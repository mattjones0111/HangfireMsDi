namespace HangfireDi
{
    public class ADummyJob
    {
        private readonly IDependency _dependency;

        public ADummyJob(IDependency dependency)
        {
            _dependency = dependency;
        }

        public void Go()
        {
            _dependency.Go();
        }
    }
}
