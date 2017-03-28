using System;

using juba.tfs.interfaces;

using Microsoft.TeamFoundation.Client;


namespace juba.tfs.wrappers
{
    public class TfsTeamProjectCollectionWrapper : ITfsTeamProjectCollection
    {
        private readonly TfsTeamProjectCollection myTfsTeamProjectCollection;

        public TfsTeamProjectCollectionWrapper(Uri uri)
        {
            myTfsTeamProjectCollection= TfsTeamProjectCollectionFactory.GetTeamProjectCollection(uri);
        }
        public T GetService<T>()
        {
            return myTfsTeamProjectCollection.GetService<T>();
        }
    }
}
