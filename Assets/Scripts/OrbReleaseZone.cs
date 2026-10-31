
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OrbReleaseZone : MonoBehaviour
{
    // Quantas partes da região estão sendo ocupadas
    // por cada Collider pertencente ao jogador.
    private readonly Dictionary<Collider, int> sobreposicoes = new();

    public bool JogadorDentro => sobreposicoes.Count > 0;


    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;

        BoxCollider[] caixas = GetComponents<BoxCollider>();

        if (caixas.Length == 0)
        {
            Debug.LogError(
                "OrbReleaseZone precisa de pelo menos um Box Collider.",
                this
            );
        }

        foreach (BoxCollider caixa in caixas)
        {
            if (!caixa.isTrigger)
            {
                Debug.LogWarning(
                    "Um dos Box Colliders da OrbReleaseZone não está como Trigger.",
                    caixa
                );
            }
        }
    }


    private bool EhJogador(Collider outro)
    {
        return outro.GetComponentInParent<PlayerOrbInteraction>() != null;
    }


    private void OnTriggerEnter(Collider outro)
    {
        if (!EhJogador(outro))
            return;

        bool estavaDentro = JogadorDentro;

        if (sobreposicoes.ContainsKey(outro))
        {
            sobreposicoes[outro]++;
        }
        else
        {
            sobreposicoes.Add(outro, 1);
        }

        if (!estavaDentro)
        {
            Debug.Log("Jogador entrou na zona de entrega!");
        }
    }


    private void OnTriggerExit(Collider outro)
    {
        if (!EhJogador(outro))
            return;

        if (!sobreposicoes.ContainsKey(outro))
            return;

        sobreposicoes[outro]--;

        if (sobreposicoes[outro] <= 0)
        {
            sobreposicoes.Remove(outro);
        }

        if (!JogadorDentro)
        {
            Debug.Log("Jogador saiu da zona de entrega!");
        }
    }


    private void OnDisable()
    {
        sobreposicoes.Clear();
    }
}
