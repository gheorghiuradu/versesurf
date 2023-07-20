using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FirestoreExtensions
{
    public static class FirestoreExtensions
    {
        public static IEnumerable<DocumentReference> Documents(this CollectionReference collection, IEnumerable<string> ids)
        {
            return ids.Select(id => collection.Document(id));
        }

        public static async ValueTask<IEnumerable<DocumentSnapshot>> GetSnapshotsAsync(this IEnumerable<DocumentReference> documents)
        {
            var results = new List<DocumentSnapshot>();
            foreach (var document in documents)
            {
                var result = await document.GetSnapshotAsync();
                results.Add(result);
            }

            return results;
        }

        public static IEnumerable<T> ConvertTo<T>(this QuerySnapshot documents)
        {
            return documents.Select(doc => doc.ConvertTo<T>());
        }
    }
}