using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Migrator.Common
{
    public class MailProviderFactory
    {
        /// <remarks>
        /// This is not a production code, it was just the simplest way to load providers from external assemblies.
        /// </remarks>
        private static readonly IEnumerable<IMailProvider> mailProviders = Assembly.GetEntryAssembly()!.GetReferencedAssemblies().SelectMany(a =>
            Assembly.Load(a.ToString()).GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IMailProvider).IsAssignableFrom(t))
                .Select(t => (IMailProvider)Activator.CreateInstance(t)!)).ToArray();

        public static IMailProvider? GetMailProvier(string name)
        {
            return mailProviders.FirstOrDefault(mp => mp.Name == name);
        }
    }
}
