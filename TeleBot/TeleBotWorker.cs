using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace TeleBot
{
    class TeleBotWorker
    {
        private static string botToken;
        private static bool consoleSilentMode;

        public TeleBotWorker(string _botToken, bool _consoleSilentMode)
        {
            botToken = _botToken;
            consoleSilentMode = _consoleSilentMode;
        }

        public enum StatusEnvio
        {
            AguardandoEnviar = -1,
            Enviado,
            Erro
        };
        public static StatusEnvio statusEnvio;

        public enum StatusReceber
        {
            AguandandoReceber = -1,
            RecebimentoConcluido,
            Erro

        };
        public static StatusReceber statusReceber;

        private static readonly HttpClient client = new HttpClient();

        public static async void GetMessages()
        {
            string message_id = string.Empty;
            string first_name = string.Empty;
            string last_name = string.Empty;
            string username = string.Empty;
            string chat_id = string.Empty;
            string text = string.Empty;
            string dtMsg;

            string urlTelegramChat = "https://api.telegram.org/bot" + botToken + "/getUpdates";  // para receber  

            HttpClient cliente = new HttpClient();
            string msgHeader = "\"result\":";

            string response = await cliente.GetStringAsync(urlTelegramChat);

            int startPoint = response.IndexOf(msgHeader) + msgHeader.Length;
            string TelegramMessageArray = response.Substring(startPoint);
            TelegramMessageArray = TelegramMessageArray.Substring(0, TelegramMessageArray.Length - 1);

            int arrayCount = TelegramMessageArray.Split(new string[] { "message_id" }, StringSplitOptions.None).Length - 1;

            var result = (dynamic)JArray.Parse(TelegramMessageArray);
            if (arrayCount > 0)
            {
                if (arrayCount == 1)
                {
                    if (!consoleSilentMode)
                        Console.WriteLine("Uma nova atualização recebida:");
                }

                else
                {
                    if (!consoleSilentMode)
                        Console.WriteLine(arrayCount.ToString() + " Novas atualizações recebidas:");
                }


                for (int i = 0; i < arrayCount; i++)
                {
                    message_id = (result[i].message.message_id?.ToString());
                    first_name = (result[i].message.from.first_name?.ToString());
                    last_name = (result[i].message.from.last_name?.ToString());
                    username = (result[i].message.from.username?.ToString());

                    chat_id = (result[i].message.chat.id?.ToString());

                    text = (result[i].message.text?.ToString());
                    dtMsg = result[i].message.date.ToString();

                    // if(!consoleSilentMode) {}
                    Program.ConsolePrint(message_id + "; " + chat_id + "; " + first_name + "; " + last_name + "; " + username + "; " + text + "\r\n", false);
                    Program.ConsolePrint("-----------------------------------------------------" + "\r\n", false);
                }
            }
            else
            {
                // if(!consoleSilentMode)
                Program.ConsolePrint("Sem novas Mensagens :-(" + "\r\n", false);
            }

            statusReceber = StatusReceber.RecebimentoConcluido;
        }

        public static void DoPost(string chatId, string textMsg)
        {
            string urlTelegramPost = "https://api.telegram.org/bot";
            PostAsync(urlTelegramPost + botToken + "/" + "sendMessage?chat_id=" + chatId + "&text=" + textMsg.Replace(" ", "%20"), "");
        }

        private static async void PostAsync(string uri, string data)
        {
            var httpClient = new HttpClient();

            var getRequest = await httpClient.GetAsync(uri);
            if (!consoleSilentMode)
                Console.WriteLine("> PostAsync -> " + uri + "  ");
            try
            {
                if (getRequest.EnsureSuccessStatusCode().IsSuccessStatusCode)
                {
                    string response = await getRequest.Content.ReadAsStringAsync();

                    var JsonResponseDynamic = (dynamic)JObject.Parse(response);

                    if (bool.Parse(JsonResponseDynamic.ok.ToString()))
                    {
                        if (!consoleSilentMode)
                            Console.WriteLine("Sucesso no Envio");
                        statusEnvio = StatusEnvio.Enviado;
                    }
                }
                else
                {
                    if (!consoleSilentMode)
                        Console.WriteLine("> ERRO no request, uri: \r\n" + uri);
                    statusEnvio = StatusEnvio.Erro;
                }
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                if (!consoleSilentMode)
                    Console.WriteLine("> ERRO no request, uri: \r\n" + uri);
            }
            catch (Exception e)
            {
                if (!consoleSilentMode)
                    Console.WriteLine("> ERRO no request " + e.ToString() + "\r\n uri: " + uri);
            }

        }

    }
}
