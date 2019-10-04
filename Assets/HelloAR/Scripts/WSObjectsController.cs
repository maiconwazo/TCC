using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WSObjectsController : MonoBehaviour
{
	public GameObject Cylinder;
	public class GameObjectGPS
	{
		public string identificador;
		public GameObject objeto;
		public float longitude;
		public float latitude;
		public bool posicionado;

		public override string ToString()
		{
			return $"Guid: {identificador}\r\nLatitude: {latitude}\r\nLongitude: {longitude}\r\nPosicionado: {(posicionado ? "Sim" : "Nao")}";
		}
	}

	private void Start()
	{
		float lat = 26.9165f;
		float longi = 49.0718f;
		//AdicionarObjeto(lat, longi);	
	}

	public List<GameObjectGPS> ListaObjetos = new List<GameObjectGPS>();

	public void AdicionarObjeto(float _lat, float _longitude)
	{
		GameObject myCylinder = Instantiate(Cylinder, new Vector3(0, 0.1f, 0), Quaternion.identity) as GameObject;
		ListaObjetos.Add(new GameObjectGPS() { identificador = Guid.NewGuid().ToString(), objeto = myCylinder, longitude = _longitude, latitude = _lat, posicionado = false });
	}

	public List<GameObjectGPS> retornarListaObjetosProximo(float latAtual, float longAtual)
	{
		float latInicial = latAtual - 0.1f;
		float latFinal = latAtual + 0.1f;
		float longInicial = longAtual - 0.1f;
		float longFinal = longAtual + 0.1f;

		//obj.latitude >= latInicial && obj.latitude <= latFinal && obj.longitude >= longInicial && obj.longitude <= latFinal && 
		return ListaObjetos.Where(obj => !obj.posicionado).ToList();
	}
}
