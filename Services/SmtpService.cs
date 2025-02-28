using System.Net.Sockets;
using System.Text;
using log4net;

public class SmtpService {
    private readonly ILog log;
    private readonly TcpClient client;
    private int emailNo = 0;
    private int threadNo = 0;

    public SmtpService(TcpClient client, ILog log, int threadNo) {
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
        NetworkStream clientStream = client.GetStream();
        ASCIIEncoding encoder = new();
        byte[] buffer = encoder.GetBytes(strMessage + "\r\n");
        clientStream.Write(buffer, 0, buffer.Length);
        clientStream.Flush();
    }

    private string Read() {
        byte[] messageBytes = new byte[8192];
        NetworkStream clientStream = client.GetStream();
        ASCIIEncoding encoder = new();
        int bytesRead = clientStream.Read(messageBytes, 0, 8192);
        string strMessage = encoder.GetString(messageBytes, 0, bytesRead);
        return strMessage;
    }
}