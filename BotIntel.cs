using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BotIntel : MonoBehaviour
{
    public Card BotAcao(int jogadorBot, List<Card> cartasJogadas, Jogador[] jogadores, int rodadaNumero)
    {
        
        var cartasEmOrdemDojogador = jogadores[jogadorBot].cartasDisponiveis.OrderByDescending(carta => carta.ValorDeDesempate).ToArray();
        if(rodadaNumero == 1)
        {
            var menorCartaDesempate = cartasEmOrdemDojogador.First();
            return menorCartaDesempate;
        }
        else if (cartasJogadas.Count() == 0)
        {
            var menorCartaDesempate = cartasEmOrdemDojogador.Last();
            return menorCartaDesempate;

        }else if (cartasJogadas.Count() == 1)
        {
            
            for (int i = jogadores[jogadorBot].cartasDisponiveis.Count()-1; i >= 0; i--) {
                if (cartasJogadas[0].Valor < cartasEmOrdemDojogador[i].Valor)
                {
                    var maiorCartaDesempate = cartasEmOrdemDojogador[i];
                    return maiorCartaDesempate;
                }
            }
            var menorCartaDesempate = cartasEmOrdemDojogador.Last();
            return menorCartaDesempate;
        }
        else
        {
            var cartasDeMaiorValorJogadas = cartasJogadas.OrderByDescending(carta => carta.Valor).ToArray();
            var indiceJogador = -2;
            var maiorCarta = cartasDeMaiorValorJogadas.First();

            foreach (var jogador in jogadores)
            {
                bool cartaEncontrada = jogador.playerHand.Any(carta => carta.Key == maiorCarta.Key);
                if (cartaEncontrada)
                {
                    indiceJogador = Array.IndexOf(jogadores, jogador);
                    break;
                }
            }

            if (jogadores[indiceJogador].team == jogadores[jogadorBot].team)
            {
                var menorCartaDesempate = cartasEmOrdemDojogador.Last();
                return menorCartaDesempate;
            }
            else{

                for (int i = jogadores[jogadorBot].cartasDisponiveis.Count()-1; i >= 0; i--)
                {
                    if (maiorCarta.Valor < cartasEmOrdemDojogador[i].Valor)
                    {
                        var maiorCartaDesempate = cartasEmOrdemDojogador[i];
                        return maiorCartaDesempate;
                    }
                }
                var menorCartaDesempate = cartasEmOrdemDojogador.Last();
                return menorCartaDesempate;
            }
        }
    }
}
