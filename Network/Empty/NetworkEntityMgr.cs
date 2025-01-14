// using System.Collections.Generic;
//
// namespace Network
// {
//     public class NetworkEntityMgr
//     {
//         public readonly Dictionary<uint,NetworkEntity> ownedEntities;
//         public NetworkEntityMgr()
//         {
//             ownedEntities = new Dictionary<uint, NetworkEntity>();
//         }
//
//         public void Add(NetworkEntity entity)
//         {
//             ownedEntities.Add(entity.id,entity);
//         }
//         
//         public void Remove(NetworkEntity entity)
//         {
//             ownedEntities.Remove(entity.id);
//         }
//
//         public void Clear()
//         {
//             ownedEntities.Clear();
//         }
//     }   
// }