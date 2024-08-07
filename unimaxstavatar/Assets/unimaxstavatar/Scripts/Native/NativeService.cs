using System.Collections.Generic;

namespace Maxst.Avatar 
{
    public class NativeService : MaxstUtils.Singleton<NativeService>
    {
        private List<INative> observers = new List<INative>();
        
        public void RegisterObserver(INative observer)
        {
            observers.Add(observer);
        }

        public void UnregisterObserver(INative observer)
        {
            observers.Remove(observer);
        }

        public void Init()
        {
            foreach (var observer in observers)
            {
                observer.Init();
            }
        }

        public void Close()
        {
           foreach(var observer in observers)
            {
                observer.Close();
            }
        }
    }
}


