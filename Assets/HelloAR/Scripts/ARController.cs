using GoogleARCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
#endif

public class ARController : MonoBehaviour
{
	private List<DetectedPlane> m_NewTrackedPlanes = new List<DetectedPlane>();
	public GameObject GridPrefab;
	public GameObject Cylinder;
	public Text Log;
	public WSObjectsController store;
	public Camera mainCamera;

	private LocationInfo lastData;
	private int qtdObjetos = 0;
	private float anguloNorte = 0;
	private Vector3 objPos;

	List<string> objetosProcessados = new List<string>();

	void Start()
	{
		UnityEngine.Input.compass.enabled = true;
		var location = UnityEngine.Input.location;
		location.Start();

		int maxWait = 20;
		while (location.status == LocationServiceStatus.Initializing && maxWait > 0)
		{
			new WaitForSeconds(1);
			maxWait--;
		}

		if (maxWait < 1)
		{
			print("Timed out");
		}
	}

	void Update()
	{
		try
		{
			if (Session.Status != SessionStatus.Tracking)
			{
				Log.text = Session.Status.ToString();
				return;
			}

			Session.GetTrackables<DetectedPlane>(m_NewTrackedPlanes, TrackableQueryFilter.New);
			anguloNorte = UnityEngine.Input.compass.magneticHeading;
			lastData = UnityEngine.Input.location.lastData;

			float angulo = 0;
			float distancia = 0;
			var objetosProximos = store.retornarListaObjetosProximo(LatitudeAtual(), LongitudeAtual());
			foreach (var obj in objetosProximos)
			{
				angulo = calcularAngulo(LatitudeAtual(), LongitudeAtual(), obj.latitude, obj.longitude) - anguloNorte;
				distancia = calcularDistancia(LatitudeAtual(), LongitudeAtual(), obj.latitude, obj.longitude);
				Vector3 novoVetor = Quaternion.AngleAxis(angulo, Vector3.up) * Vector3.forward * distancia;

				Pose pose = new Pose(novoVetor, Quaternion.identity);
				Anchor anchor = Session.CreateAnchor(pose);
				obj.objeto.transform.position = pose.position;
				obj.objeto.transform.rotation = pose.rotation;
				obj.objeto.transform.parent = anchor.transform;
				obj.posicionado = true;
				obj.objeto.SetActive(true);
				objPos = obj.objeto.transform.position;

				qtdObjetos++;
			}
			
			if (qtdObjetos > 0)
				Debug.DrawLine(mainCamera.transform.position, objPos, Color.red);

			Display(angulo, distancia);
		}
		catch (Exception e)
		{
			Log.text = e.Message;
		}
	}

	public float LatitudeAtual()
	{
		float latitude = 0;
		try
		{
			//latitude = lastData.latitude;
			latitude = 26.9166f;
		}
		catch { }

		return latitude;
	}

	public float LongitudeAtual()
	{
		float longitude = 0;
		try
		{
			//longitude = lastData.longitude;
			longitude = 49.0719f;
		}
		catch { }

		return longitude;
	}

	public float AnguloNorteAtual()
	{
		return anguloNorte;
	}

	private void Display(float angulo, float distancia)
	{
		Log.text = $"Angulo norte: {anguloNorte}\r\nLatitude: {LatitudeAtual()}\r\nLongitude: {LongitudeAtual()}\r\nObjetos posicionados: {qtdObjetos}\r\nUltimo objeto posicionado: {angulo}º - {distancia}m\r\nObjetos disponiveis: {store.ListaObjetos.Count}";
		//if (!erro)
		//	Log.text = $"Origem\r\nLatitude: {latOrigemStr}\r\nLongitude: {longOrigemStr}\r\nTimestamp: {timestampOrigemStr}\r\n\r\nDestino\r\nLatitude: {latDestinoStr}\r\nLongitude: {longDestinoStr}\r\nTimestamp: {timestampDestinoStr}\r\n\r\nDistância: {distanciaStr}\r\nÂngulo: {anguloStr}\r\n\r\nÂngulo norte: {anguloNorte}\r\nState: {state} ({(processando ? "Processando..." : "Pronto!")})";
	}

	private float calcularAngulo(float _latOrigem, float _longOrigem, float _latDestino, float _longDestino)
	{
		float latitude1 = DegreeToRadian(_latOrigem);
		float latitude2 = DegreeToRadian(_latDestino);
		float longDiff = DegreeToRadian(_longDestino - _longOrigem);
		float y = Mathf.Sin(longDiff) * Mathf.Cos(latitude2);
		float x = Mathf.Cos(latitude1) * Mathf.Sin(latitude2) - Mathf.Sin(latitude1) * Mathf.Cos(latitude2) * Mathf.Cos(longDiff);

		return (RadianToDegree(Mathf.Atan2(y, x)) + 360) % 360;
	}

	private float RadianToDegree(float angle)
	{
		return angle * (180.0f / Mathf.PI);
	}

	private float calcularDistancia(float _latOrigem, float _longOrigem, float _latDestino, float _longDestino)
	{
		const int R = 6371; // Radius of the earth

		float latDistance = DegreeToRadian(_latDestino - _latOrigem);
		float lonDistance = DegreeToRadian(_longDestino - _longOrigem);
		float a = Mathf.Sin(latDistance / 2) * Mathf.Sin(latDistance / 2)
				+ Mathf.Cos(DegreeToRadian(_latOrigem)) * Mathf.Cos(DegreeToRadian(_latDestino))
				* Mathf.Sin(lonDistance / 2) * Mathf.Sin(lonDistance / 2);
		float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
		float distance = R * c * 1000;

		float height = 1 - 1;

		distance = Mathf.Pow(distance, 2) + Mathf.Pow(height, 2);

		return Mathf.Sqrt(distance);
	}

	private float DegreeToRadian(float angle)
	{
		return Mathf.PI * angle / 180.0f;
	}
}
