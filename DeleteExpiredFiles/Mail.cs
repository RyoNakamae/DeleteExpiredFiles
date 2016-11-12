using System.Text;
using DeleteExpiredFiles.Properties;

public class Mail
{
    /// <summary>
    /// メール送信宛先省略版
    /// </summary>
    /// <param name="subject">タイトル</param>
    /// <param name="body">本文</param>
    public static void SendMail(string subject, string body)
    {
        string mailTo = Settings.Default.MailTo;
        string mailCc = null;
        SendMail(mailTo, mailCc, subject, body);
    }

    public static void SendMail(string mailTo, string subject, string body)
    {
        string mailCc = null;
        SendMail(mailTo, mailCc, subject, body);
    }

    /// <summary>
    /// メール送信処理
    /// </summary>
    /// <param name="mailTo">To送信者</param>
    /// <param name="mailCc">Cc送信者</param>
    /// <param name="subject">タイトル</param>
    /// <param name="body">メール本文</param>
    /// <param name="strErr">エラーメッセージ</param>
    public static void SendMail(string mailTo, string mailCc, string subject, string body)
    {
        string server = Settings.Default.MailServer;
        string user = Settings.Default.MailUser;
        string pass = Settings.Default.MailPass;
        int port = Settings.Default.MailPort;
        string mailFrom = Settings.Default.MailFrom;

        System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(server, port);
        System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(mailFrom, mailTo);
        if (mailCc != null)
        {
            message.CC.Add(mailCc);
        }


        if (Settings.Default.DevFlg) subject += "<<検証環境>>";
        message.Subject = subject;

        //署名を追加
        body += "\r\n" + DeleteExpiredFiles.Program.MakeBatchMessage();
        message.Body = body;

        message.BodyEncoding = Encoding.GetEncoding("utf-8");
        message.SubjectEncoding = Encoding.GetEncoding("utf-8");

        client.Credentials = new System.Net.NetworkCredential(user, pass);

        client.Send(message);
    }
}
