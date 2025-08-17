using ApiPayments.RequisitionsModels.Payloads;
using ApiPaymets.Database.Entities;
using System.Threading.Channels;

namespace ApiPaymets.Channels
{

    public class PaymentChannel
    {
        private readonly Channel<PaymentPayloadModel> _channel;
        
        public PaymentChannel()
        {
            _channel = Channel.CreateUnbounded<PaymentPayloadModel>();
        }

        public ChannelWriter<PaymentPayloadModel> Writer => _channel.Writer;
        public ChannelReader<PaymentPayloadModel> Reader => _channel.Reader;
    }
}
