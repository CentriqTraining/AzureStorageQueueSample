using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorageQueueSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //  There are actually two components of a Storage Queue.
            //      The Client (which will SEND the message)
            //      The Server (Which will receive the message)
            //////////////////////////////////////////
            //  BOTH WILL USE CONNECTION COMPONENTS //
            //////////////////////////////////////////

            //  TEST DATA
            //  create an xml object to send from the client portion
            XElement xmlemp = new XElement("Employee",
                                        new XElement("ID", 5),
                                        new XElement("LastName", "Rissen"),
                                        new XElement("FirstName", "Mike"));

            #region Connection Components
            //  Get the azure connection strong for azure storage
            //  Set your settings in the app.config from azure management portal
            string con = ConfigurationManager.ConnectionStrings["AzureConnection"].ConnectionString;

            // Create a Storage client from connection string
            var storeClient = CloudStorageAccount.Parse(con);

            //  Create a queue client
            var queueClient = storeClient.CreateCloudQueueClient();

            //  Get a reference to a specific queue
            var queue = queueClient.GetQueueReference("stufftodo");
            queue.CreateIfNotExists();
            queue.Clear();
            #endregion

            #region Client Components
            //  Client would use this code to add a message to the queue
            queue.AddMessage(new CloudQueueMessage(xmlemp.ToString()));
            #endregion

            #region Server Components
            //  Queue worker would use this code to get the next message
            //  This gets the message and makes it invisible to others for 30sec.
            var msg = queue.GetMessage();

            //  This gets the body as either a string or byte[]
            var xmlemp2 = msg.AsString;

            //  There is no complete() - use deletemessage when you are done
            //  If you don't call delete message, it goes back into the queue when the lease expires.
            queue.DeleteMessage(msg);


            //  Updates attributes of this queue (better message count)
            queue.FetchAttributes();
            var cnt = queue.ApproximateMessageCount;

            //  You can also get the message without locking it
            var msg2 = queue.PeekMessage();

            //  To deserialize the item into xml again, copy it into a memeory stream
            //  To do this, you use a bytearray containing the string
            var xmlemp3b = msg2.AsBytes;

            //  Feed it to a new memorystream
            var memstrm = new MemoryStream(xmlemp3b);

            //  Load it into the XElement
            var xmlemp3 = XElement.Load(memstrm);
            //  You can also save it to a file using a filestream instead.
            #endregion
        }
    }
}
