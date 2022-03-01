using System.Diagnostics;

namespace HangfireDi
{
    public class ConcreteDependency : IDependency
    {
        public void Go()
        {
            Debug.WriteLine("Hi there");
        }
    }
}