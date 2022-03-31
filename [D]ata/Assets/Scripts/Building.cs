using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
	private enum Align { Enabled, Disabled }

	[SerializeField] private Align alignment = Align.Enabled; // выравнивание позиции, т.е. до целых чисел
	[SerializeField] private LayerMask layerMask; // фильтр, где можно строиться
	[SerializeField] private RectTransform IconsRect; // окно, в котором находятся иконки, что была невозможно установка если курсор в этом окне
	[SerializeField] private Color lockColor; // цвет, если невозможно установить
	[SerializeField] private Color unlockColor; // цвет, если возможно установить
	[SerializeField] private float heightOffset; // сдвиг по высоте для 3D, может понадобится, чтобы моделька не "висела в воздухе" или наоборот если они слишком "уходят под землю"

	// папки в Resources где лежат превью и оригинальные префабы
	private string prefabPath = "Prefabs";
	private static string previewPath = "Preview";

	private static BuildingPreview target;
	private static GameObject _lastTarget;
	private static bool _active;
	private bool m_2d, canUse;
	private static LayerMask _ignoreMask;
	private static string curName;
	private static float cutTime;

	public static bool isActive
	{
		get { return _active; }
	}

	public static GameObject lastTarget
	{
		get { return _lastTarget; }
	}

	public static LayerMask ignoreLayers
	{
		get { return _ignoreMask; }
	}

	public static void LoadPreview(string objName)
	{
		if (target != null) DestroyPreview();

		curName = objName;
		string path = previewPath + "/" + objName;
		BuildingPreview obj = Resources.Load<BuildingPreview>(path);

		if (obj)
		{
			_active = true;
			target = Instantiate(obj);
		}
		else Error(path);
	}

	public static void ResetStatus()
	{
		cutTime = 0;
	}

	void Awake()
	{
		_active = false;
		_ignoreMask = ~layerMask;
		m_2d = Camera.main.orthographic;
	}

	Vector3 GetMousePosition()
	{
		Vector3 pos = Vector3.zero;

		if (!m_2d)
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) pos = hit.point;
		}
		else
		{
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, layerMask);
			if (hit.transform != null) pos = hit.point;
		}

		if (alignment == Align.Enabled)
		{
			pos.x = Mathf.Round(pos.x);

			if (m_2d)
			{
				pos.y = Mathf.Round(pos.y);
			}
			else
			{
				pos.z = Mathf.Round(pos.z);
				pos.y = pos.y + heightOffset;
			}
		}

		return pos;
	}

	bool CheckLayer() // доп. проверка при установки модели, чтобы нельзя было установить одну, внутри другой
	{
		int index = 0;

		if (!m_2d)
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit) && !hit.collider.isTrigger) index = hit.collider.gameObject.layer;
		}
		else
		{
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
			if (hit.transform != null && !hit.collider.isTrigger) index = hit.collider.gameObject.layer;
		}

		if (((1 << index) & ignoreLayers) != 0)
		{
			return true;
		}

		return false;
	}

	bool IsOverlap()
	{
		Vector2 mouse = Input.mousePosition;
		Vector3[] worldCorners = new Vector3[4];
		IconsRect.GetWorldCorners(worldCorners);

		if (mouse.x >= worldCorners[0].x && mouse.x < worldCorners[2].x
			&& mouse.y >= worldCorners[0].y && mouse.y < worldCorners[2].y)
		{
			return true;
		}

		if (CheckLayer()) return true;

		return false;
	}

	Vector3 PositionCorrection(Vector3 position)
	{
		if (!m_2d)
		{
			target.transform.position = position + new Vector3(0, 100, 0);
			RaycastHit hit;
			if (Physics.Linecast(position, target.transform.position, out hit, 1 << 2))
			{
				return new Vector3(position.x, target.transform.position.y - hit.distance, position.z);
			}
		}

		return position;
	}

	static void DestroyPreview()
	{
		Destroy(target.gameObject);
		_active = false;
	}

	void TargetStatus()
	{
		cutTime += Time.deltaTime;
		if (cutTime > .1f)
		{
			canUse = true;
			target.SetColor(unlockColor);
		}
		else
		{
			canUse = false;
			target.SetColor(lockColor);
		}
	}

	void TargetRotation()
	{
		if (Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			if (m_2d) target.transform.Rotate(0, 0, 90); else target.transform.Rotate(0, 90, 0);
		}
		else if (Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			if (m_2d) target.transform.Rotate(0, 0, -90); else target.transform.Rotate(0, -90, 0);
		}
	}

	void Update()
	{
		if (target == null) return;

		TargetStatus();

		Vector3 position = GetMousePosition();
		target.transform.position = PositionCorrection(position);

		TargetRotation();

		if (Input.GetMouseButtonDown(0) && !IsOverlap() && canUse)
		{
			string path = prefabPath + "/" + curName;
			GameObject obj = Resources.Load<GameObject>(path);
			if (obj) _lastTarget = Instantiate(obj, target.transform.position, target.transform.rotation) as GameObject; else Error(path);
			DestroyPreview();
		}
		else if (Input.GetMouseButtonDown(1))
		{
			DestroyPreview();
		}
	}

	static void Error(string val)
	{
		Debug.Log("[Building] указанного объекта не существует: Resources/" + val + ".prefab");
	}
}
