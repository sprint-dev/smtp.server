using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using log4net;
using log4net.Util;

public class SmtpServiceFail {
    private readonly ILog log;
    private readonly TcpClient client;
    private int emailNo = 0;
    private int threadNo = 0;

    public SmtpServiceFail(TcpClient client, ILog log, int threadNo) {
        this.log = log;
        this.client = client;
        this.threadNo = threadNo;
    }

    public void Run() {
        Write("220 localhost -- Fake proxy server");
        while (true) {
            string strMessage;
            try {
                strMessage = Read();
                log.Info($"READ MESSAGE {strMessage}, ({strMessage.Length}), {threadNo}");
            } catch (Exception ex) {
                Write("221 Bye");
                client.Close();
                log.Info($"READ EXCEPTION {ex.Message}, {threadNo}");
                break;
            }

            if (strMessage.Length == 0) {
                log.Info($"MESSAGE EMPTY, {threadNo}");
                Write("250 OK");
                continue;
            }

            if (strMessage.StartsWith("QUIT") || strMessage.TrimEnd().EndsWith("QUIT")) {
                try {
                    Write("221 Bye");
                    client.Close();
                    log.Info($"MESSAGE QUIT, {threadNo}");
                    break;//exit while

                } catch (Exception ex) {
                    log.Info($"MESSAGE QUIT Exception {ex.Message}, {threadNo}");
                    break;
                }
            }

            if (strMessage.StartsWith("EHLO")) {
                log.Info($"MESSAGE EHLO, {threadNo}");
                Write("250 OK");
                continue;
            }

            if (strMessage.StartsWith("RCPT TO")) {
                log.Info($"MESSAGE RCPT TO, {threadNo}");
                Write("250 OK");
                continue;
            }

            if (strMessage.StartsWith("MAIL FROM") || strMessage.Contains("MAIL FROM")) {
                emailNo++;
                log.Info($"MESSAGE MAIL FROM, {threadNo}");
                Write("250 OK");
                continue;
            }

            if (strMessage.StartsWith("DATA")) {
                log.Info($"MESSAGE DATA, {threadNo}");
                Write("354 Start mail input; end with");
                strMessage = Read();
                if (threadNo > 10 && threadNo%11 == 0) {
                    Thread.Sleep(1000);
                    log.Info($"TIMEOUT, {strMessage},  {threadNo}");
                    Write("421 4.4.1 Connection timed out. Total session duration: 00:10:01.8904373");
                    //Thread.Sleep(100000);
                    break;
                }
                log.Info($"MESSAGE BODY, {strMessage},  {threadNo}");
                Write("250 OK");
                continue;
            }

            if (strMessage.Length != 0) {
                log.Info($"MESSAGE BODY, {strMessage},  {threadNo}");
            }
        }
    }

    private void Write(string strMessage) {
        try {
            NetworkStream clientStream = client.GetStream();
            ASCIIEncoding encoder = new();
            byte[] buffer = encoder.GetBytes(strMessage + "\r\n");
            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        } catch (Exception ex) {
            string mess = ex.Message;
        }
    }

    private string Read() {
        try {
            byte[] messageBytes = new byte[8192];
            NetworkStream clientStream = client.GetStream();
            ASCIIEncoding encoder = new();
            int bytesRead = clientStream.Read(messageBytes, 0, 8192);
            string strMessage = encoder.GetString(messageBytes, 0, bytesRead);
            return strMessage;
        } catch (Exception ex) {
            return ex.Message;
        }
    }
}