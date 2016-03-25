using System.Reflection;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace DataFlowWPF
{

    public static class MEF
    {
        private static CompositionContainer _container;

        static MEF()
        {
            var catalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            _container = new CompositionContainer(catalog);
        }

        public static T GetInstance<T>()
        {
            return _container.GetExportedValue<T>();
        }

        public static void SatisfyImportsOnce(object part)
        {
            _container.SatisfyImportsOnce(part);

        }
    }
}
