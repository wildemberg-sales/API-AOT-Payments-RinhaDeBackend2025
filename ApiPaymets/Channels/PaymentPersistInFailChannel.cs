using ApiPayments.RequisitionsModels.Payloads;
using ApiPaymets.Database.Entities;
using System.Threading.Channels;

namespace ApiPaymets.Channels
{
    public class PaymentPersistInFailChannel
    {
        private readonly Channel<Payment> _channel;

        public PaymentPersistInFailChannel()
        {
            _channel = Channel.CreateUnbounded<Payment>();
        }

        public ChannelWriter<Payment> Writer => _channel.Writer;
        public ChannelReader<Payment> Reader => _channel.Reader;
    }
}
