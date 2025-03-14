using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;

namespace VirtualQueueApi.ExternalServices;

public class EmailService
{    
    public static void SendEmail(string receiverEmail, string receiverName, string subject, string message)
    {
        var apiInstance = new TransactionalEmailsApi();
        SendSmtpEmailSender sender = new SendSmtpEmailSender("", "");
        SendSmtpEmailTo receiver1 = new SendSmtpEmailTo(receiverEmail, receiverName);
        List<SendSmtpEmailTo> To = new List<SendSmtpEmailTo>();
        To.Add(receiver1);

        string HtmlContent = null;
        string TextContent = message;
        
        try
        {
            var sendSmtpEmail = new SendSmtpEmail(sender, To, bcc: null, cc: null, htmlContent: null, 
                textContent: TextContent, subject: subject, replyTo: null, attachment: null, headers: null, 
                templateId: null,_params:null, messageVersions: null, tags: null);
            CreateSmtpEmail result = apiInstance.SendTransacEmail(sendSmtpEmail);            
            Console.WriteLine("Response " + result.ToJson());            
        }
        catch (Exception e)
        {
            Console.WriteLine("we have an exception: " + e.Message);
        }
    }

    public static void SendEmail(string receiverEmail, string receiverName, long templateId, object parameters)
    {
        var apiInstance = new TransactionalEmailsApi();
        SendSmtpEmailSender sender = new SendSmtpEmailSender("", "");
        SendSmtpEmailTo receiver1 = new SendSmtpEmailTo(receiverEmail, receiverName);
        List<SendSmtpEmailTo> To = new List<SendSmtpEmailTo>();
        To.Add(receiver1);
                
        try
        {
            var sendSmtpEmail = new SendSmtpEmail(sender, To, templateId: templateId, _params: parameters);
            CreateSmtpEmail result = apiInstance.SendTransacEmail(sendSmtpEmail);
            Console.WriteLine("Response " + result.ToJson());
        }
        catch (Exception e)
        {
            Console.WriteLine("we have an exception: " + e.Message);
        }
    }

    public static void SendWelcomeEmail(string receiverEmail, string receiverName, string companyReceiverName)
    {
        WelcomeEmailParams parameters = new WelcomeEmailParams() { name = receiverName, companyName = companyReceiverName }; 
        var templateId = 1;        
        SendEmail(receiverEmail, receiverName, templateId, parameters);
    }

    public static void SendResetCode(string receiverEmail, string receiverName, string code)
    {
        ForgotPasswordParams parameters = new ForgotPasswordParams() { name = receiverName, code = code };
        var templateId = 3;
        SendEmail(receiverEmail, receiverName, templateId, parameters);
    }

    public static void SendStripeWebHook(string json, string eventType)
    {
        SendEmail("", "", eventType, json);
    }
}

public class WelcomeEmailParams
{
    public string name { get; set; }
    public string companyName { get; set; }
}

public class ForgotPasswordParams
{
    public string name { get; set; }
    public string code { get; set; }
}