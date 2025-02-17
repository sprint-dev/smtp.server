using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using log4net;
using log4net.Config;



class Program {
    private static readonly ILog log = LogManager.GetLogger(typeof(Program));
    static void Main() {
        try {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));

            Console.WriteLine("Service start!");
            log.Info("SERVICE START!");
            int threadNo = 0;
            IPEndPoint endPoint = new(IPAddress.Any, 25);
            TcpListener listener = new(endPoint);
            listener.Start();

            while (true) {
                try {
                    TcpClient client = listener.AcceptTcpClient();
                    threadNo++;
                    SmtpServiceFail handler = new(client, log, threadNo);
                    log.Info($"START LISTEN");
                    Thread thread = new(new ThreadStart(handler.Run));
                    thread.Start();
                } catch (Exception ex) {
                    log.Info($"THREAD RESTART {ex.Message}");
                    continue;
                }
            }
        } catch (Exception ex) {
            log.Info($"Error {ex.Message}");
        }
    }


}
