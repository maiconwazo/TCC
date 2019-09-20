using System;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
	public InputField fieldLatitude;
	public InputField fieldLongitude;
	public Text aviso;
	public GameObject painel;
	public WSObjectsController store;
	public Text consulta;
	public GameObject scrollView;
	public ARController arcoreController;

	private void Start()
	{
		painel.SetActive(false);
		scrollView.SetActive(false);
	}

	public void AdicionarObjeto()
	{
		string latText = fieldLatitude.text;

		float latitude = 0;
		try
		{
			latitude = float.Parse(latText);
		}
		catch
		{
			aviso.text = "Erro ao converter latitude.";
			fieldLatitude.text = "";
			painel.SetActive(true);
		}

		string longText = fieldLongitude.text;

		float longitude = 0;
		try
		{
			longitude = float.Parse(longText);
		}
		catch
		{
			aviso.text = "Erro ao converter longitude.";
			fieldLongitude.text = "";
			painel.SetActive(true);
		}

		store.AdicionarObjeto(latitude, longitude);	

		aviso.text = "";
		fieldLongitude.text = "";
		fieldLatitude.text = "";
		painel.SetActive(false);
	}

	public void ExibirPainel()
	{
		aviso.text = "";
		fieldLongitude.text = arcoreController.LongitudeAtual().ToString();
		fieldLatitude.text = arcoreController.LatitudeAtual().ToString();
		painel.SetActive(true);
	}

	public void ConsultarObjetos()
	{
		consulta.text = "";
		foreach (WSObjectsController.GameObjectGPS obj in store.ListaObjetos)
		{
			if (!String.IsNullOrWhiteSpace(consulta.text))
				consulta.text += "\r\n\r\n";

			consulta.text += obj.ToString();
		}
		
		scrollView.SetActive(true);
	}

	public void FecharConsulta()
	{
		consulta.text = "";
		scrollView.SetActive(false);
	}

	public void FecharCadastro()
	{
		aviso.text = "";
		fieldLongitude.text = "";
		fieldLatitude.text = "";
		painel.SetActive(false);
	}
}
