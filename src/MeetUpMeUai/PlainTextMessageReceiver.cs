using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using MeetUpMeUai.Model;
using MeetUpMeUai.Persistence;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace MeetUpMeUai
{
    public class PlainTextMessageReceiver : IMessageReceiver
    {
        private readonly IMessagingHubSender _sender;

        public PlainTextMessageReceiver(IMessagingHubSender sender)
        {
            _sender = sender;
            DocumentDBRepository<Interaction>.Initialize();
        }

        public async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            Guid id = Guid.NewGuid();
            var item = new Interaction()
            {
                Id = id.ToString(),
                Identifier = message.From.ToString(),
                Message = message.Content.ToString()
            };
            
            //create 
            await DocumentDBRepository<Interaction>.CreateItemAsync(item);
            
            var items = await DocumentDBRepository<Interaction>.GetItemsAsync(i => i.Identifier == item.Identifier);
            
            
            var replyMessage = string.Empty;
            foreach (var interaction in items)
            {
                replyMessage = replyMessage + "\n" + interaction.Message;
            }

            var toReply = string.Format("Desculpe! Enquanto eu não estou pronto, vou te imitar:\n{0}", replyMessage);

            Console.WriteLine(string.Format("From: {0} \tContent: {1}", message.From, message.Content));
            await _sender.SendMessageAsync("SmartContact is Under Construction! =D. See you later!", message.From, cancellationToken);
            await _sender.SendMessageAsync(toReply, message.From, cancellationToken);
        }
    }
}
