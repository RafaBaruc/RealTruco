using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Equipe
{
    public int pontosPartida;
    public int pontosRodada;
    public int pontosSubRodada;
    public Jogador[] jogadoresEquipe;
}
public class Jogador
{
    public int id; //para identificar os 4 jogadorem, de 0 a 3
    public int team;//time do jogador 0 a 1
    public bool isBot;
    public int numDeCartas;
    public Card[] playerHand;//cartas que estão na mao do jogador de 0 a 2
    public Transform[] MaosJogadores;
    public List<Card> cartasDisponiveis; // Lista de cartas disponíveis para jogar

    // Método para remover uma carta da lista de cartas disponíveis
    public void RemoverCartaDisponivel(Card carta)
    {
        if (cartasDisponiveis.Contains(carta))
        {
            cartasDisponiveis.Remove(carta);
        }
    }
}
public class Rodada
{   
    public bool comecouRodada;
    public int numRodada;
    public int subRodada;//Vai só até 3
    public int vezDoJogador;//existem 4 jogadorem, de 0 a 3
}

public class Baralho
{
    public Card[] Cards;//Vai só até 27
    public Card[] CardsOnTable;//Vai só até 4
}
public class Card
{
    public string Key;
    public int Valor;
    public int ValorDeDesempate;
    // Outros dados necessários, como sprites, materiais, etc.
}


public class GameManager : MonoBehaviour
{
    public delegate void CartaClicadaEventHandler(int indiceCarta);
    public static event CartaClicadaEventHandler OnCartaClicada;

    public Dictionary<int, string> cartasBaralho = new Dictionary<int, string>();
    public Dictionary<string, string> cardSprites = new Dictionary<string, string>();
    public Dictionary<string, string> cardMaterials = new Dictionary<string, string>(); // Dicionário para mapear as cartas aos materiais

    public BotIntel ai;
    public Equipe[] equipes;
    public Transform[] maosJogadores;
    public Transform[] pontosMesa; // Array de pontos na mesa para posicionar as cartas
    public Button botaoIniciar;
    public Button botaoReiniciar;
    public GameObject Iniciar;
    public GameObject telaDoJogo;
    public GameObject telaFinal;
    public bool comecouPartida;
    public GameObject cardPrefab;
    private bool aguardandoClique = false;
    private bool aguardandoCliqueTruco = false;
    public TMPro.TextMeshProUGUI PP;
    public TMPro.TextMeshProUGUI PR;
    public TMPro.TextMeshProUGUI VaRodada;
    Baralho baralho = new Baralho();
    public List<Card> cartasNaMesa = new List<Card>();

    public TMPro.TextMeshProUGUI logText;
    public TMPro.TextMeshProUGUI logTextFinal;
    public bool fugiu;
    public bool aceitou;
    public int vez;
    public int ValorRodada;
    public int Truco;
    public int subRodada;
    public int rodada;
    public Button aceitarButton;
    public Button aumentarButton;
    public Button trucoButton;
    public Button voltarButton;
    public ScrollRect scrollRect;
    public Image suaVezImage;
    public Animator[] animator;

    public bool suaVez;
    public Image[] cardImagesUI; // Array de imagens para representar as cartas na UI
    Rodada rodadaa = new Rodada();
    Jogador[] jogadores;

    public int valorMin = 1;
    public int valorMax = 27;

    public int numeroSorteado;

    public List<int> numerosJaSorteados = new List<int>();

    List<int> numeros = new List<int>();
    void GerarNumerosAleatorios()
    {
        if (numeros.Count == 0)
        {
            Debug.LogWarning("A lista de números está vazia. Reiniciando...");

            // Reinicializar a lista de números
            numeros.AddRange(Enumerable.Range(valorMin, valorMax - valorMin + 1));
        }

        int indice = UnityEngine.Random.Range(0, numeros.Count);
        numeroSorteado = numeros[indice];
        numerosJaSorteados.Add(numeroSorteado);
        numeros.RemoveAt(indice);
    }
    void Start()
    {
        voltarButton.onClick.AddListener(() => OnButtonClick(99));
        baralho.Cards = new Card[27];
        baralho.CardsOnTable = new Card[4];
        LoadCartasDobaralho();
        // Carregar sprites das cartas
        LoadCardSprites();
        // Carregar materiais das cartas
        LoadCardMaterials();

        for (int x = valorMin; x <= valorMax; x++)
        {
            numeros.Add(x);
        }

        jogadores = new Jogador[4];
        int k = 0;
        for (int i = 0; i < 4; i++)
        {
            
            jogadores[i] = new Jogador(); // Inicializa cada jogador antes de acessar seus campos
            jogadores[i].playerHand = new Card[3]; // Inicializa o array playerHand de cada jogador
            jogadores[i].MaosJogadores = new Transform[3];
            if (i == 1 || i == 3) 
            {
                jogadores[i].team = 1;
            }
            else
            {
                jogadores[i].team = 0;
            }

            for (int j = 0; j < 3; j++)
            {
                GerarNumerosAleatorios(); // Adicionado aqui para gerar números aleatórios
                string key = cartasBaralho[numeroSorteado]; // Obtém a chave da carta
                if (i == 0)
                {
                    jogadores[i].isBot = false; // Neste caso em especifico o player 0 é jogador controlavel e o resto é bot
                }
                else
                {
                    jogadores[i].isBot = true;
                }
               
                jogadores[i].MaosJogadores[j] = maosJogadores[k];
                k++;
                jogadores[i].numDeCartas++;
                jogadores[i].playerHand[j] = baralho.Cards[numeroSorteado - 1];
            }
        }

        equipes = new Equipe[2]; // Supondo que haja duas equipes

        // Inicialize cada equipe
        for (int i = 0; i < equipes.Length; i++)
        {
            equipes[i] = new Equipe();
            equipes[i].pontosPartida = 0;
            equipes[i].pontosRodada = 0;
            equipes[i].pontosSubRodada = 0;
            // Adicione os jogadores à equipe
            equipes[i].jogadoresEquipe = new Jogador[2]; // Supondo que cada equipe tenha dois jogadores

            // Atribua os jogadores existentes às equipes
            for (int j = 0; j < 2; j++) // Supondo que você tenha dois jogadores existentes para cada equipe
            {
                // Atribua o jogador existente à equipe
                equipes[i].jogadoresEquipe[j] = jogadores[i * 2 + j]; // Supondo que os jogadores estejam em um array chamado "jogadores"
            }
        }

        for (int i = 0; i < jogadores.Length; i++)
        {
            jogadores[i].cartasDisponiveis = new List<Card>(jogadores[i].playerHand);
        }

        k = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (jogadores[i].isBot == false)
                {
                    MostrarCartaUI(jogadores[i].playerHand[j].Key, cardImagesUI[k]);
                }
                k++;
            }
        }

        k = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            { 
                MostrarCartaMao(jogadores[i].playerHand[j].Key, k);
                k++;
            }
        }

        comecouPartida = false;
        // Adiciona um listener para o evento de clique do botão
        botaoIniciar.onClick.AddListener(Iniciarpartida);

    }
    void Inicializar()
    {
        numeros.Clear();
        for (int x = valorMin; x <= valorMax; x++)
        {
            numeros.Add(x);
        }
        numeroSorteado = 0;
        numerosJaSorteados.Clear();

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                GerarNumerosAleatorios(); // Adicionado aqui para gerar números aleatórios
                jogadores[i].playerHand[j] = null;
                jogadores[i].numDeCartas++;
                jogadores[i].playerHand[j] = baralho.Cards[numeroSorteado - 1];
            }
        }
        for (int i = 0; i < jogadores.Length; i++)
        {
            jogadores[i].cartasDisponiveis = new List<Card>(jogadores[i].playerHand);
        }
        int k = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                MostrarCartaMao(jogadores[i].playerHand[j].Key, k);
                k++;
            }
        }

        for (int i = 0; i < jogadores[0].playerHand.Length; i++)
        {
            cardImagesUI[i].gameObject.SetActive(true);
        }

        MostrarCartaUI(jogadores[0].playerHand[0].Key, cardImagesUI[0]);
        MostrarCartaUI(jogadores[0].playerHand[1].Key, cardImagesUI[1]);
        MostrarCartaUI(jogadores[0].playerHand[2].Key, cardImagesUI[2]);
    }

    private void Update()
    {
        PP.text = equipes[0].pontosRodada.ToString() + " : " + equipes[1].pontosRodada.ToString();
        PR.text = equipes[0].pontosSubRodada.ToString() + " : " + equipes[1].pontosSubRodada.ToString();
        int ValorRodadaW;
        if (Truco == 0)
        {
            ValorRodadaW = 1;
        }
        else
        {
            ValorRodadaW = Truco * 3;
        }
        VaRodada.text = "A rodada vale " + ValorRodadaW.ToString();
    }
    private IEnumerator ExecutarPartida()
    {
        comecouPartida = true;
        int foot = 0;
        int jogadorComMaiorCarta = 0;
        aceitarButton.gameObject.SetActive(false);
        aumentarButton.gameObject.SetActive(false);
        PP.text = equipes[0].pontosRodada.ToString() + " : " + equipes[1].pontosRodada.ToString();
        for (rodada = 1; rodada <= 25 && equipes[0].pontosRodada < 12 && equipes[1].pontosRodada < 12; rodada++)
        {
            Debug.Log("Começou a rodada " + rodada);

            if (rodada != 1)
            {
                Inicializar();
            }
            
            if (foot == 3)
            {
                foot = 0;
            }
            else
            {
                foot++;
            }
            trucoButton.gameObject.SetActive(true);
            jogadorComMaiorCarta = foot;
            Truco = 0;
            fugiu = false;
            ValorRodada = 1;
            for (subRodada = 1; subRodada <= 3; subRodada++)
            {
                aceitarButton.gameObject.SetActive(false);
                aumentarButton.gameObject.SetActive(false);
                Debug.Log("Começou a subrodada " + subRodada);
                Debug.Log("Pontos de subRodada da equipe0 " + equipes[0].pontosSubRodada + " Pontos de subRodada da equipe1 " + equipes[1].pontosSubRodada);
                PR.text = equipes[0].pontosSubRodada.ToString() + " : " + equipes[1].pontosSubRodada.ToString();
                Debug.Log("Pontos de Rodada da equipe0 " + equipes[0].pontosRodada + " Pontos de Rodada da equipe1 " + equipes[1].pontosRodada);
                PP.text = equipes[0].pontosRodada.ToString() + " : " + equipes[1].pontosRodada.ToString();
                // Inicialmente, definimos o jogador com a maior carta como o primeiro jogador
                for (vez = 0; vez < 4; vez++)
                {
                    int jogadorAtual = (vez + jogadorComMaiorCarta) % 4;
                    rodadaa.vezDoJogador = jogadorAtual;
                    jogadorAtual = (jogadorComMaiorCarta == -1)? 0 : jogadorAtual;
                    LogMessage("Vez do jogador " +  (jogadorAtual + 1));
                    if (jogadores[jogadorAtual].isBot)
                    {
                        yield return new WaitForSeconds(1);
                        yield return new WaitUntil(() => !aguardandoCliqueTruco);
                        JogarCartaNaMesa(jogadorAtual, 0);
                        yield return new WaitForSeconds(1);
                    }
                    else
                    {
                        // É um jogador humano, aguardar o clique da carta
                        aguardandoClique = true;
                        //textMeshProUGUI.text = "Sua Vez";
                        suaVezImage.gameObject.SetActive(true);
                        // Aguardar até que o jogador clique em uma carta
                        yield return new WaitUntil(() => !aguardandoClique);
                        yield return new WaitUntil(() => !aguardandoCliqueTruco);
                        //textMeshProUGUI.text = ""; // Limpar o texto após a jogada do jogador
                    }
                    suaVezImage.gameObject.SetActive(false);
                }
                if (fugiu == false)
                {
                    //Verifica a maior carta
                    yield return new WaitForSeconds(1);
                    jogadorComMaiorCarta = VerificarMaiorCarta(subRodada - 1, cartasNaMesa);
                    yield return new WaitForSeconds(1);
                }

                LimparMesa();
                cartasNaMesa.Clear();
            }
        }

        // Fim da partida
        Debug.Log("Fim da partida");
        LogMessage("Fim da partida.");
        if (equipes[0].pontosRodada >= 12)
        {
            LogMessage("Equipe 1 ganhou a partida com 12 x" + equipes[1].pontosRodada + " pontos!");
        } else if(equipes[1].pontosRodada >= 12)
        {
            LogMessage("Equipe 2 ganhou a partida com 12 x" + equipes[0].pontosRodada + " pontos!");
        }
        yield return new WaitForSeconds(1);
        telaDoJogo.SetActive(false);
        telaFinal.SetActive(true);
        LogMessageReset();
        botaoReiniciar.onClick.AddListener(reinicia);
        if (equipes[0].pontosRodada >= 12)
        {
            LogMessageFinal("Equipe 1 ganhou a partida com 12 x " + equipes[1].pontosRodada + " pontos!", 0);
        }
        else if (equipes[1].pontosRodada >= 12)
        {
            LogMessageFinal("Equipe 2 ganhou a partida com 12 x" + equipes[0].pontosRodada + " pontos!", 1);
        }
    }

    void reinicia()
    {
        SceneManager.LoadScene(1);
    }
    void Iniciarpartida()
    {
        // Coloque aqui a lógica para iniciar a partida
        Debug.Log("A partida começou!");
        Iniciar.SetActive(false);
        telaFinal.SetActive(false);
        telaDoJogo.SetActive(true);
        comecouPartida = true;
        rodada = 1;
        // Inicialize cada equipe
        for (int i = 0; i < equipes.Length; i++)
        {
            equipes[i].pontosPartida = 0;
            equipes[i].pontosRodada = 0;
            equipes[i].pontosSubRodada = 0;
        }
        Inicializar();
        StopCoroutine(ExecutarPartida());
        StartCoroutine(ExecutarPartida());
    }

    void JogarCartaNaMesa(int jogador, int indiceBtn)
    {
        // Verificar se o jogador é um bot
        if (jogadores[jogador].isBot)
        {
            // Verificar se o jogador tem cartas na mão
            if (jogadores[jogador].cartasDisponiveis.Count > 0)
            {
                ai = gameObject.AddComponent<BotIntel>();
                // Gerar um índice aleatório dentro do intervalo válido das cartas disponíveis
                Card CartaAjogar; 
                if ( (equipes[0].pontosSubRodada == 5 && equipes[1].pontosSubRodada == 5) || (equipes[0].pontosSubRodada == 10 && equipes[1].pontosSubRodada == 10))
                {
                    CartaAjogar = ai.BotAcao(jogador, cartasNaMesa, jogadores, 1);
                }
                else
                {
                    CartaAjogar = ai.BotAcao(jogador, cartasNaMesa, jogadores, 0);
                }

                int indiceCarta = 0;

                for (int i = 0; i < jogadores[jogador].cartasDisponiveis.Count(); i++)
                {
                    // Verificar se alguma carta da mão do jogador é igual à maior carta
                    if (jogadores[jogador].playerHand[i] == CartaAjogar)
                    {
                        indiceCarta = i;
                        break;
                    }
                }

                // Obter a carta que será jogada
                Card cartaJogada = jogadores[jogador].cartasDisponiveis[indiceCarta];

                // Mostrar a carta na mesa na posição do jogador
                MostrarCartaMesa(cartaJogada.Key, jogador);

                // Remover a carta da lista de cartas disponíveis do jogador
                jogadores[jogador].RemoverCartaDisponivel(cartaJogada);

                // Remover a carta da mão do jogador
                RemoverCartaDaMao(cartaJogada.Key, jogador, indiceCarta);

                // Adicionar a carta à mesa
                cartasNaMesa.Add(cartaJogada);

                LogMessage("O jogador " + (jogador+1) + " jogou a carta " + jogadores[jogador].playerHand[indiceCarta].Key);
                // Outras operações...
            }
            else
            {
                Debug.LogWarning("O jogador " + jogador + " não tem cartas disponíveis para jogar.");
            }
        }
        else
        {
            Card cartaJogada = jogadores[jogador].playerHand[indiceBtn];
            cardImagesUI[indiceBtn].gameObject.SetActive(false); // Supondo que queremos esconder a primeira carta na 

            // Remover a carta da mão do jogador
            RemoverCartaDaMao(jogadores[0].playerHand[indiceBtn].Key, 0, indiceBtn); // Supondo que queremos remover a carta "4 de Paus" da mão do jogador 1

            // Mostrar a carta na mesa
            MostrarCartaMesa(jogadores[0].playerHand[indiceBtn].Key, 0); // Supondo que queremos mostrar a carta "4 de Paus" na mesa na direção do jogador 1
            cartasNaMesa.Add(cartaJogada);
        }
    }
    private int VerificarMaiorCarta(int subRodada, List<Card> cartasJogadas)
    {
        
        if (cartasJogadas == null || cartasJogadas.Count == 0)
        {
            return -1; // Retorna uma tupla com null e -1 se a lista estiver vazia
        }

        // Ordena as cartas pelo valor em ordem decrescente
        var cartasOrdenadas = cartasJogadas.OrderByDescending(carta => carta.Valor);

        // A primeira carta na lista ordenada será a maior
        var maiorCarta = cartasOrdenadas.First();
        var segundaCarta = cartasOrdenadas.Skip(1).First();

        var cartasDeDesempate = cartasJogadas.OrderByDescending(carta => carta.ValorDeDesempate);
        var maiorCartaDesempate = cartasDeDesempate.First();
       

        // Encontra o índice do jogador que jogou a maior carta
        int indiceJogador = -2;
        int indiceJogadorDesempate = -2;
        int indiceJogadorSegundo = -2;

        foreach (var jogador in jogadores)
        {
            bool cartaEncontrada = jogador.playerHand.Any(carta => carta.Key == maiorCarta.Key);
            if (cartaEncontrada)
            {
                indiceJogador = Array.IndexOf(jogadores, jogador);
                break;
            }
        }
        Debug.Log(indiceJogador);
        for (int i = 0; i < jogadores.Length; i++)
        {
            // Verificar se alguma carta da mão do jogador é igual à maior carta
            if (jogadores[i].playerHand.Any(carta => carta.Key == segundaCarta.Key))
            {
                indiceJogadorSegundo = i;
                break;
            }
        }
        foreach (var jogador in jogadores)
        {
            bool cartaEncontrada = jogador.playerHand.Any(carta => carta.Key == maiorCartaDesempate.Key);
            if (cartaEncontrada)
            {
                indiceJogadorDesempate = Array.IndexOf(jogadores, jogador);
                break;
            }
        }
        int rodadaResult = -1;
        if (indiceJogador != 0 && indiceJogador != 1 && indiceJogador != 2 && indiceJogador != 3)
        {
            LogMessage("maior carta: " + maiorCarta.Key);
            LogMessage("segunda maior carta: " + segundaCarta.Key);
            LogMessage("maior carta desempate: " + maiorCartaDesempate.Key);
            LogMessage("mesa " + cartasJogadas[0].Key + " " + cartasJogadas[1].Key + " " + cartasJogadas[2].Key + " " + cartasJogadas[3].Key);
            for (int i = 0; i < 4; i++)
            {
                LogMessage("Jogador " + i + " tem as cartas: " + jogadores[i].playerHand[0].Key + " " + jogadores[i].playerHand[1].Key + " " + jogadores[i].playerHand[2].Key + " ");
            }
        }
        if (maiorCarta.Valor == segundaCarta.Valor && jogadores[indiceJogador].team != jogadores[indiceJogadorSegundo].team)
        {
            rodadaResult = 2;
        }
        else if (jogadores[indiceJogador].team == 1)
        {
            rodadaResult = 1;
        }
        else if (jogadores[indiceJogador].team == 0)
        {
            rodadaResult = 0;
        }
       
        Pontuar(rodadaResult, subRodada, indiceJogadorDesempate) ;
        // Limpar as cartas da mesa após verificar a maior carta
        
        cartasNaMesa.Clear();
        
        LogMessage("A maior carta é a do jogador " + (indiceJogador+1));
        return indiceJogadorDesempate;
    }

    void Pontuar(int rodadaResult, int rodadaNum, int jogadorDesempate)
    {
       
        if(Truco == 0)
        {
            ValorRodada = 1;
        }
        else
        {
            ValorRodada = Truco * 3;
        }

        if (rodadaResult == 0)
        {
            if (rodadaNum == 0)
            {
                equipes[0].pontosSubRodada += 6;
            }
            else
            {
                equipes[0].pontosSubRodada += 5;
            }

        }
        else if (rodadaResult == 1)
        {
            if (rodadaNum == 0)
            {
                equipes[1].pontosSubRodada += 6;
            }
            else
            {
                equipes[1].pontosSubRodada += 5;
            }
        }
        else
        {
            equipes[0].pontosSubRodada += 5;
            equipes[1].pontosSubRodada += 5;
        }

        if (rodadaNum == 1 || rodadaNum == 2)
        {
            if (equipes[0].pontosSubRodada >= 10 && equipes[1].pontosSubRodada < 10 || (equipes[0].pontosSubRodada == 11 && equipes[1].pontosSubRodada == 10))
            {
                equipes[0].pontosRodada += ValorRodada;
                equipes[0].pontosSubRodada = 0;
                equipes[1].pontosSubRodada = 0;
                Truco = 0;
                ValorRodada = 0;
                subRodada = 4;
            }
            else if (equipes[1].pontosSubRodada >= 10 && equipes[0].pontosSubRodada < 10 || (equipes[1].pontosSubRodada == 11 && equipes[0].pontosSubRodada == 10))
            {
                equipes[1].pontosRodada += ValorRodada;
                equipes[0].pontosSubRodada = 0;
                equipes[1].pontosSubRodada = 0;
                Truco = 0;
                ValorRodada = 0;
                subRodada = 4;
            }
            else if(equipes[0].pontosSubRodada == 15 && equipes[1].pontosSubRodada == 15 )
            {
                if (jogadores[jogadorDesempate].team == 0)
                {
                    equipes[0].pontosRodada += ValorRodada;
                    equipes[0].pontosSubRodada = 0;
                    equipes[1].pontosSubRodada = 0;
                    Truco = 0;
                    ValorRodada = 0;
                    subRodada = 4;
                }
                else if (jogadores[jogadorDesempate].team == 1)
                {
                    equipes[1].pontosRodada += ValorRodada;
                    equipes[0].pontosSubRodada = 0;
                    equipes[1].pontosSubRodada = 0;
                    Truco = 0;
                    ValorRodada = 0;
                    subRodada = 4;
                }
            }
        }
    }

    void MostrarCartaUI(string key, Image cardImage)
    {
        // Obter o caminho do sprite correspondente à carta
        string spritePath = cardSprites[key];

        // Carregar o sprite da carta
        Sprite spriteCarta = Resources.Load<Sprite>(spritePath);

        // Atualizar o sprite da imagem na UI
        cardImage.sprite = spriteCarta;
    }

    void LoadCartasDobaralho()
    {
        AddCarta(1, "JOuros", 1);
        AddCarta(2, "JEspadas", 1);
        AddCarta(3, "JCopas", 1);
        AddCarta(4, "JPaus", 1);
        AddCarta(5, "QOuros", 2);
        AddCarta(6, "QEspadas", 2);
        AddCarta(7, "QCopas", 2);
        AddCarta(8, "QPaus", 2);
        AddCarta(9, "KOuros", 3);
        AddCarta(10, "KEspadas", 3);
        AddCarta(11, "KCopas", 3);
        AddCarta(12, "KPaus", 3);
        AddCarta(13, "AOuros", 4);
        AddCarta(14, "ACopas", 4);
        AddCarta(15, "APaus", 4);
        AddCarta(16, "2Ouros", 5);
        AddCarta(17, "2Espadas", 5);
        AddCarta(18, "2Copas", 5);
        AddCarta(19, "2Paus", 5);
        AddCarta(20, "3Ouros", 6);
        AddCarta(21, "3Espadas", 6);
        AddCarta(22, "3Copas", 6);
        AddCarta(23, "3Paus", 6);
        AddCarta(24, "7Ouros", 7);
        AddCarta(25, "AEspadas", 8);
        AddCarta(26, "7Copas", 9);
        AddCarta(27, "4Paus", 10);
    }

    void LoadCardSprites()
    {
        // Carregar sprites das cartas e associá-las às suas strings correspondentes
        AddSprite("4Paus", "Sprites/Club04");
        AddSprite("7Copas", "Sprites/Heart07");
        AddSprite("AEspadas", "Sprites/Spade01");
        AddSprite("7Ouros", "Sprites/Diamond07");
        AddSprite("3Paus", "Sprites/Club03");
        AddSprite("3Copas", "Sprites/Heart03");
        AddSprite("3Espadas", "Sprites/Spade03");
        AddSprite("3Ouros", "Sprites/Diamond03");
        AddSprite("2Paus", "Sprites/Club02");
        AddSprite("2Copas", "Sprites/Heart02");
        AddSprite("2Espadas", "Sprites/Spade02");
        AddSprite("2Ouros", "Sprites/Diamond02");
        AddSprite("APaus", "Sprites/Club01");
        AddSprite("ACopas", "Sprites/Heart01");
        AddSprite("AOuros", "Sprites/Diamond01");
        AddSprite("KPaus", "Sprites/Club13");
        AddSprite("KCopas", "Sprites/Heart13");
        AddSprite("KEspadas", "Sprites/Spade13");
        AddSprite("KOuros", "Sprites/Diamond13");
        AddSprite("QPaus", "Sprites/Club12");
        AddSprite("QCopas", "Sprites/Heart12");
        AddSprite("QEspadas", "Sprites/Spade12");
        AddSprite("QOuros", "Sprites/Diamond12");
        AddSprite("JPaus", "Sprites/Club11");
        AddSprite("JCopas", "Sprites/Heart11");
        AddSprite("JEspadas", "Sprites/Spade11");
        AddSprite("JOuros", "Sprites/Diamond11");
        // Implemente conforme sua necessidade
    }

    void LoadCardMaterials()
    {
        // Carregar materiais das cartas e associá-los às suas strings correspondentes
        AddMaterial("4Paus", "Materials/Red_PlayingCards_Club04_00");
        AddMaterial("7Copas", "Materials/Red_PlayingCards_Heart07_00");
        AddMaterial("AEspadas", "Materials/Red_PlayingCards_Spade01_00");
        AddMaterial("7Ouros", "Materials/Red_PlayingCards_Diamond07_00");
        AddMaterial("3Paus", "Materials/Red_PlayingCards_Club03_00");
        AddMaterial("3Copas", "Materials/Red_PlayingCards_Heart03_00");
        AddMaterial("3Espadas", "Materials/Red_PlayingCards_Spade03_00");
        AddMaterial("3Ouros", "Materials/Red_PlayingCards_Diamond03_00");
        AddMaterial("2Paus", "Materials/Red_PlayingCards_Club02_00");
        AddMaterial("2Copas", "Materials/Red_PlayingCards_Heart02_00");
        AddMaterial("2Espadas", "Materials/Red_PlayingCards_Spade02_00");
        AddMaterial("2Ouros", "Materials/Red_PlayingCards_Diamond02_00");
        AddMaterial("APaus", "Materials/Red_PlayingCards_Club01_00");
        AddMaterial("ACopas", "Materials/Red_PlayingCards_Heart01_00");
        AddMaterial("AOuros", "Materials/Red_PlayingCards_Diamond01_00");
        AddMaterial("KPaus", "Materials/Red_PlayingCards_Club13_00");
        AddMaterial("KCopas", "Materials/Red_PlayingCards_Heart13_00");
        AddMaterial("KEspadas", "Materials/Red_PlayingCards_Spade13_00");
        AddMaterial("KOuros", "Materials/Red_PlayingCards_Diamond13_00");
        AddMaterial("QPaus", "Materials/Red_PlayingCards_Club12_00");
        AddMaterial("QCopas", "Materials/Red_PlayingCards_Heart12_00");
        AddMaterial("QEspadas", "Materials/Red_PlayingCards_Spade12_00");
        AddMaterial("QOuros", "Materials/Red_PlayingCards_Diamond12_00");
        AddMaterial("JPaus", "Materials/Red_PlayingCards_Club11_00");
        AddMaterial("JCopas", "Materials/Red_PlayingCards_Heart11_00");
        AddMaterial("JEspadas", "Materials/Red_PlayingCards_Spade11_00");
        AddMaterial("JOuros", "Materials/Red_PlayingCards_Diamond11_00");
    }
    void AddMaterial(string key, string materialPath)
    {
        if (!cardMaterials.ContainsKey(key))
        {
            cardMaterials.Add(key, materialPath);
        }
        else
        {
            Debug.LogWarning("Chave duplicada no dicionário de materiais: " + key);
        }
    }

    void AddSprite(string key, string spritePath)
    {
        if (!cardSprites.ContainsKey(key))
        {
            cardSprites.Add(key, spritePath);
        }
        else
        {
            Debug.LogWarning("Chave duplicada no dicionário de sprites: " + key);
        }
    }
    void AddCarta(int number, string key, int valor)
    {
        // Adicionar a carta aos dicionários dos jogadores
        cartasBaralho.Add(number, key);

        // Adicionar os valores e valores de desempate à carta
        Card carta = new Card();
        carta.Key = key;
        carta.Valor = valor;
        carta.ValorDeDesempate = number;

        // Adicionar a carta ao baralho
        baralho.Cards[number - 1] = carta;
    }
    public void OnButtonClick(int indiceBtn)
    {   if (indiceBtn == 99) {
            SceneManager.LoadScene(0);
        }
        else if (indiceBtn == 11 && aguardandoClique) // 11 é o índice do botão de fugir da equipe 0
        {
            FugirDaEquipe(1); // Chama a função para a equipe 0 fugir
            LogMessage("A equipe 2 fugiu.");
            aguardandoCliqueTruco = false;
        }
        else if (indiceBtn == 10 && aguardandoClique) // 10 é o índice do botão de fugir da equipe 1
        {
            FugirDaEquipe(0); // Chama a função para a equipe 1 fugir
            LogMessage("A equipe 1 fugiu.");
            aguardandoCliqueTruco = false;
        }
        else if (indiceBtn == 21 && aguardandoClique)
        {
            LogMessage("A equipe 2 pede TRUCO!!");
            aceitarButton.gameObject.SetActive(false);
            aumentarButton.gameObject.SetActive(false);
            aguardandoCliqueTruco = false;
            PedirTruco(1);
        }
        else if (indiceBtn == 20 && aguardandoClique)
        {
            LogMessage("A equipe 1 pede TRUCO!!");
            aceitarButton.gameObject.SetActive(false);
            aumentarButton.gameObject.SetActive(false);
            aguardandoCliqueTruco = false;
            PedirTruco(0);
        }
        else if (indiceBtn == 31 && !aceitou)
        {
                LogMessage("A equipe 2 aceitou.");
                aceitarButton.gameObject.SetActive(false);
                aumentarButton.gameObject.SetActive(false);
                aceitou = true;
                aguardandoCliqueTruco = false;
                Truco++;
        }
        else if (indiceBtn == 30 && !aceitou)
        {
                LogMessage("A equipe 1 aceitou.");
                aceitarButton.gameObject.SetActive(false);
                aumentarButton.gameObject.SetActive(false);
                aceitou = true;
                aguardandoCliqueTruco = false;
                Truco++;
        }
        else if (indiceBtn == 41 && !aceitou)
        {
                aceitarButton.gameObject.SetActive(false);
                aumentarButton.gameObject.SetActive(false);
                aceitou = true;
                aguardandoCliqueTruco = false;
                Truco++;
                LogMessage("A equipe 2 aumenta para " + ((Truco +1) *3));
                PedirTruco(1);
        }
        else if (indiceBtn == 40 && !aceitou)
        {
            LogMessage("A equipe 1 amenta.");
            aceitarButton.gameObject.SetActive(false);
            aumentarButton.gameObject.SetActive(false);
            aceitou = true;
            aguardandoCliqueTruco = false;
            Truco++;
            LogMessage("A equipe 1 aumenta para " + ((Truco + 1) * 3));
            PedirTruco(0);
        }
        else if (aguardandoClique && !aguardandoCliqueTruco)
        {
            LogMessage("O jogador 1 jogou a carta " + jogadores[0].playerHand[indiceBtn].Key);
            JogarCartaNaMesa(0, indiceBtn);
            OnCartaClicada?.Invoke(indiceBtn);
            aguardandoClique = false;
        }
        
    }

    private void PedirTruco(int equipe)
    {
        if(equipes[1].pontosRodada == 11 || equipes[0].pontosRodada == 11)
        {
            // Fim da partida
            Debug.Log("Fim da partida");
            LogMessage("Fim da partida.");
           
            telaDoJogo.SetActive(false);
            telaFinal.SetActive(true);
            LogMessageReset();
            botaoReiniciar.onClick.AddListener(reinicia);

            if (equipe == 0)
            {
                LogMessageFinal("Equipe 1 ganhou a partida pois a Equipe 2 pediu truco na mão de 11",0);
                
            }
            else if (equipe == 1)
            {
                LogMessageFinal("Equipe 2 ganhou a partidapois a Equipe 1 pediu truco na mão de 11", 1);
                
            }
        }
        // Interrompe a corrotina
        aceitou = false;
        if (equipe == 0)
        {
            
            ApresentarOpcoesTruco(1);
        }else if (equipe == 1)
        {
            
            ApresentarOpcoesTruco(0);
        }
        
    }
    private void ApresentarOpcoesTruco(int equipe)
    {
        trucoButton.gameObject.SetActive(false);
        for (int i = 0; i <= 1; i++)
        {
            if (jogadores[i].team == equipe)
            {
                if (jogadores[i].isBot)
                {
                    int botAction = UnityEngine.Random.Range(0, 3);
                    if (botAction == 0)
                    {
                        Debug.Log("A equipe " + equipe + " aceitou o truco.");
                        LogMessage("A equipe " + (equipe+1) + " aceitou.");
                        Truco++;
                        aguardandoCliqueTruco = false;
                    }
                    else if (botAction == 1)
                    {
                        Debug.Log("A equipe " + equipe + " fugiu do truco.");
                        LogMessage("A equipe " + (equipe + 1) + " fugiu.");
                        FugirDaEquipe(jogadores[i].team);
                        Truco = 0;
                        aguardandoCliqueTruco = false;
                        cartasNaMesa.Clear();
                    }
                    else if (botAction == 2)
                    {
                        // Aumentar o valor do truco e mostrar novamente o botão de truco
                        if (Truco < 3)
                        {
                            Truco++;
                        }
                        else if (Truco == 4)
                        {
                            break;
                        }
                        LogMessage("A equipe " + (equipe + 1) + " pede " + ((Truco + 1) * 3));
                        PedirTruco(jogadores[i].team);
                    }
                }
                else
                {
                    aceitarButton.gameObject.SetActive(true);
                    if (Truco < 3) { 
                        aumentarButton.gameObject.SetActive(true);
                    }
                    aceitarButton.onClick.AddListener(() => OnButtonClick(30 + equipe));
                    aumentarButton.onClick.AddListener(() => OnButtonClick(40 + equipe));
                    aguardandoCliqueTruco = true;
                }
            }

        }
        
    }

    private void FugirDaEquipe(int equipeIndex)
    {
        // Define os pontos da sub-rodada para a equipe que está fugindo e para a outra equipe
        equipes[equipeIndex].pontosSubRodada = 0;
        equipes[1 - equipeIndex].pontosSubRodada = 10;
        vez = 5;
        fugiu = true;
        aguardandoClique = false;
        

        // Chama a função para pontuar e determinar a vitória da outra equipe
        Pontuar(1 - equipeIndex, 2, 1 - equipeIndex);
    }

    void RemoverCartaDaMao(string key, int jogador, int indice)
    {
        // Remover a carta da mão do jogador
        // Verificar se o jogador é válido
        if (jogador < 0 || jogador >= jogadores.Length)
        {
            Debug.LogWarning("Jogador inválido: " + jogador);
            return;
        }

        // Verificar se o índice é válido
        if (indice < 0 || indice >= jogadores[jogador].playerHand.Length)
        {
            Debug.LogWarning("Índice de carta inválido para o jogador " + jogador + ": " + indice);
            return;
        }

        // Verificar se a carta está na mão do jogador

      for (int i = 0; i < 3; i++) { 
            if (jogadores[jogador].playerHand[i] != null && jogadores[jogador].playerHand[i].Key.Contains(key))
            {
                // Destruir o objeto da carta
                Destroy(jogadores[jogador].MaosJogadores[i].GetChild(0).gameObject);
                // Logicamente remover a carta da mão (por exemplo, definindo como null)
                break;
            }
        }
        jogadores[jogador].numDeCartas--;
    }
    void MostrarCartaMao(string key, int jogador)
    {
        // Verificar se já existe uma carta na mão nesse ponto
        if (maosJogadores[jogador].childCount > 0)
        {
            // Se já houver uma carta na mão, destrua-a antes de instanciar uma nova
            Destroy(maosJogadores[jogador].GetChild(0).gameObject);
        }

        // Instanciar o objeto da carta na mão do jogador
        GameObject carta = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
        MeshRenderer meshRenderer = carta.GetComponent<MeshRenderer>();

        // Obter o material correspondente à carta
        string materialPath = cardMaterials[key];

        // Carregar o material da carta
        Material materialCarta = Resources.Load<Material>(materialPath);

        // Atualizar o material do objeto da carta
        meshRenderer.material = materialCarta;

        // Definir a posição correta na mão do jogador
        carta.transform.SetParent(maosJogadores[jogador]);
        carta.transform.localPosition = Vector3.zero;
        carta.transform.localRotation = Quaternion.Euler(Vector3.zero);

    }

    private void LimparMesa()
    {
        for (int i = 0; i < pontosMesa.Length; i++) { 
            // Se já houver uma carta na mesa, destrua-a antes de instanciar uma nova
            if(pontosMesa[i].childCount > 0) 
            {
                Destroy(pontosMesa[i].GetChild(0).gameObject);
            }
            
        }
    }
    void MostrarCartaMesa(string key, int jogador)
    {
        animator[jogador].SetTrigger("jogar");
        // Verificar se já existe uma carta na mesa nesse ponto
        if (pontosMesa[jogador].childCount > 0)
        {
            // Se já houver uma carta na mesa, destrua-a antes de instanciar uma nova
            Destroy(pontosMesa[jogador].GetChild(0).gameObject);
        }

        // Instanciar o objeto da carta na mesa
        GameObject carta = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
        MeshRenderer meshRenderer = carta.GetComponent<MeshRenderer>();

        // Obter o material correspondente à carta

        string materialPath = cardMaterials[key];

        // Carregar o material da carta
        Material materialCarta = Resources.Load<Material>(materialPath);

        // Atualizar o material do objeto da carta
        meshRenderer.material = materialCarta;

        // Define a posição correta na mesa
        carta.transform.SetParent(pontosMesa[jogador]); // Posicionar na mesa de acordo com o jogador
        carta.transform.localPosition = Vector3.zero; // Ajuste a posição conforme necessário
        carta.transform.localRotation = Quaternion.Euler(Vector3.zero);
        carta.transform.localScale = new Vector3(1, 1, 1);


        

    }

    public void LogMessage(string message)
    {
        logText.text += "> " + message + "\n";

        // Força a reconstrução do layout para garantir que o tamanho do ChatLog seja atualizado
        LayoutRebuilder.ForceRebuildLayoutImmediate(logText.rectTransform);

        // Obtém o número de linhas de texto
        int lineCount = logText.textInfo.lineCount;

        // Obtém a altura de uma linha de texto
        float lineHeight = logText.fontSize * logText.lineSpacing;

        // Calcula a altura do ChatLog com base no número de linhas de texto
        float chatLogHeight = lineCount * lineHeight;

        // Obtém o RectTransform do ChatLog
        RectTransform chatLogRectTransform = logText.rectTransform;

        // Define a altura do ChatLog com base na altura calculada
        chatLogRectTransform.sizeDelta = new Vector2(chatLogRectTransform.sizeDelta.x, chatLogHeight);

        // Move a barra de rolagem para o topo
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
    public void LogMessageReset()
    {
        logText.text = "> Nova partida iniciada.\n";
    }
    public void LogMessageFinal(string message, int equipe)
    {
        logTextFinal.text = message + "\n";

        if (equipe == 0)
        {
            logTextFinal.color = Color.white;
        }
        else
        {
            logTextFinal.color = Color.white;
        }
    }
}