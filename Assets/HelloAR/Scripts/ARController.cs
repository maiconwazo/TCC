﻿using GoogleARCore;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;

#if UNITY_EDITOR
	using Input = GoogleARCore.InstantPreviewInput;
#endif

public class ARController : MonoBehaviour
{
	private List<DetectedPlane> m_NewTrackedPlanes = new List<DetectedPlane>();
	public GameObject arCoreDevice;
	public GameObject GridPrefab;
	public GameObject Cylinder;
	public Text Log;
	public WSObjectsController store;
	public Camera mainCamera;
	public LineRenderer line;

	private LocationInfo lastData;
	private float anguloNorte = 0;
	private Vector3 objPos;
	private bool anguloConfigurado = false;

	private float anguloCalculado = 0;
	private float distancia = 0;


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

			anguloNorte = UnityEngine.Input.compass.magneticHeading;

			if (mainCamera.transform.rotation.eulerAngles.x > 180)
			{
				anguloNorte += 180;

				if (anguloNorte > 360)
					anguloNorte -= 360;
			}

			if (!anguloConfigurado)
			{
				arCoreDevice.transform.rotation = Quaternion.AngleAxis(anguloNorte, Vector3.up);
				anguloConfigurado = true;
			}

			Session.GetTrackables<DetectedPlane>(m_NewTrackedPlanes, TrackableQueryFilter.New);


			lastData = UnityEngine.Input.location.lastData;

			var objetosProximos = store.retornarListaObjetosProximo(LatitudeAtual(), LongitudeAtual());

			Session.GetTrackables<DetectedPlane>(m_NewTrackedPlanes, TrackableQueryFilter.New);
			foreach (var plane in m_NewTrackedPlanes)
			{
				var parede = Instantiate(GridPrefab, Vector3.zero, Quaternion.identity, transform);
				parede.GetComponent<GridVisualiser>().Initialize(plane);
			}

			foreach (var obj in objetosProximos)
			{
				anguloCalculado = calcularAngulo(LatitudeAtual(), LongitudeAtual(), obj.latitude, obj.longitude);

				if (anguloCalculado < 0)
					anguloCalculado += 360;

				distancia = calcularDistancia(LatitudeAtual(), LongitudeAtual(), obj.latitude, obj.longitude);
				Vector3 novoVetor = Quaternion.AngleAxis(anguloCalculado, Vector3.up) * Vector3.forward * distancia;

				Pose pose = new Pose(novoVetor, Quaternion.identity);
				Anchor anchor = Session.CreateAnchor(pose);
				obj.objeto.transform.localScale *= 0.1f;
				obj.objeto.transform.rotation = Quaternion.identity;
				obj.objeto.transform.position = pose.position;
				obj.objeto.transform.parent = anchor.transform;
				obj.posicionado = true;
				obj.objeto.SetActive(true);
				objPos = obj.objeto.transform.position;
			}

			Display();
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
			latitude = lastData.latitude;
			//latitude = 26.9166f;
		}
		catch { }

		return latitude;
	}

	public float LongitudeAtual()
	{
		float longitude = 0;
		try
		{
			longitude = lastData.longitude;
			//longitude = 49.0719f;
		}
		catch { }

		return longitude;
	}

	public float AnguloNorteAtual()
	{
		return anguloNorte;
	}

	private void Display()
	{
		Log.text = $"Angulo norte: {anguloNorte}\r\nAngulo calculado: {anguloCalculado}\r\nCamera rotation: {mainCamera.transform.localRotation.eulerAngles}\r\nCamera global rotation: {mainCamera.transform.rotation.eulerAngles}";
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
