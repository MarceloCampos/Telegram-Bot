using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Dynamic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Net;

// TeleBot - envio de msgs Telegram por http
// versão 0.1 : Ago/2019
// by Marcelo Campos : www.marcelocampos.cc
// To send the “Hello World” message using a web-browser, just open the URL:
// https://api.telegram.org/bot<TOKEN>/sendMessage?chat_id=<CHAT_ID>&text=Hello%20World

namespace TeleBot
{
    public class Program
    {

        private static string botToken = string.Empty;
        static TeleBotWorker teleBot;

        static void Main(string[] args)
        {
            int timeOut = 6000;
            int timeCount = 0;
            int timeSleep = 50; // em milisegs

            try
            {

                if (!ReadConfigs()) // lê arquivo de configurações e verifica se retornou ok (true)
                {
                    ConsolePrint("ERRO: Durante a Leitura do Arquivo de configurações (\"config.cfg\")\r\n", true);
                    return;
                }
                else if (string.IsNullOrEmpty(botToken))
                {
                    ConsolePrint("ERRO: arquivo de configurações (\"config.cfg\") Não contém o Token do Bot ...\r\n", true);
                    return;
                }
                else if (args.Length == 0)
                {
                    ConsolePrint("ERRO: Não Recebido Parâmetro de ação, tente \"Ler\" ou \"Enviar ChatId \"texto_da_mensagem\" (texto com aspas mesmo)\" ...\r\n", true);
                    return;
                }

                teleBot = new TeleBotWorker(botToken, false);

                switch (args[0].Trim().ToLower())    // verifica qual ação
                {
                    case "enviar":
                        if (args.Length != 3)
                        { ConsolePrint("ERRO: Parâmetro de ação Envio incorreto, tente: \"Enviar ChatId \"texto_da_mensagem\" (texto com aspas mesmo)\" ...\r\n", true); return; }

                        TeleBotWorker.DoPost(args[1], args[2]);

                        TeleBotWorker.statusEnvio = TeleBotWorker.StatusEnvio.AguardandoEnviar;
                        while (TeleBotWorker.statusEnvio == TeleBotWorker.StatusEnvio.AguardandoEnviar)
                        {
                            System.Threading.Thread.Sleep(timeSleep);
                            if (timeCount++ >= (timeOut / timeSleep))
                            {
                                ConsolePrint("Timeout no Envio...", false);
                                break;
                            }
                        }
                        break;

                    case "receber":
                    case "ler":
                        TeleBotWorker.statusReceber = TeleBotWorker.StatusReceber.AguandandoReceber;
                        ConsolePrint("Recebendo Atualizações Mensagens, Aguarde ...\r\n", false);
                        TeleBotWorker.GetMessages();

                        while (TeleBotWorker.statusReceber == TeleBotWorker.StatusReceber.AguandandoReceber)
                        {
                            System.Threading.Thread.Sleep(timeSleep);
                            if (timeCount++ >= (timeOut / timeSleep))
                            {
                                ConsolePrint("Timeout no Recebimento...", false);
                                break;
                            }
                        }
                        break;

                    default:
                        break;
                }

                ConsolePrint(string.Empty, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static void ConsolePrint(string strToPrint, bool aguardaTeclapSair)
        {
            Console.Write(strToPrint);

            if (aguardaTeclapSair)
            {
                Console.Write("\r\nPressione Qualquer tecla para sair...");
                Console.ReadKey();
            }
        }

        private static bool ReadConfigs()
        {
            bool ret = false;
            string fileName = "config.cfg";

            if (!File.Exists(fileName))
                return false;

            StreamReader configFile = new StreamReader(fileName);

            try
            {
                while (!configFile.EndOfStream)
                {
                    string lineRead = configFile.ReadLine();        // lê um linha do arquivo
                                                                    // se linha começa com xx ou não tem o '=' então pula
                    if (lineRead.StartsWith("#") || lineRead.StartsWith(";") || !lineRead.Contains("="))
                        continue;

                    string[] paramValor = lineRead.Split('=');

                    if (paramValor.Length != 2)   // tem sempre de ter o par: parâmetro e valor      
                        return false;

                    switch (paramValor[0].ToLower())
                    {
                        case "token":
                            botToken = paramValor[1];
                            break;
                        // ... outros parâmetros ... :
                        // case zzz :
                        default:
                            break;
                    }
                }

                configFile?.Close();
                ret = true;
            }
            catch (Exception)
            {

                configFile?.Close();
            }

            return ret;
        }

    }
}
