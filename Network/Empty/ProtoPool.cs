// using Google.Protobuf;
//
// namespace Nico
// {
//     public static class ProtoPool<TMessage> where TMessage : IMessage<TMessage>, new()
//     {
//         private static readonly Pool<TMessage> Pool = new Pool<TMessage>(() => new TMessage(), 1000);
//         public static TMessage Get() => Pool.Get();
//         public static void Return(TMessage message) => Pool.Return(message);
//     }
//     
//     public static class ProtoMessageExtensions
//     {
//         public static TMessage Get<TMessage>(this IMessage<TMessage> message) where TMessage : IMessage<TMessage>, new()
//         {
//             return ProtoPool<TMessage>.Get();
//         }
//         
//         public static void Return<TMessage>(this IMessage<TMessage> message, TMessage msg) where TMessage : IMessage<TMessage>, new()
//         {
//             ProtoPool<TMessage>.Return(msg);
//         }
//     }
// }