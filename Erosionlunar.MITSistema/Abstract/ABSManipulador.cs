using Erosionlunar.MITSistema.Interface;

namespace Erosionlunar.MITSistema.Abstract
{
    public abstract class ABSManipulador : IManipuladorArchivos
    {
        public virtual DateTime getFecha(string dirA) { return DateTime.Now; }
        protected List<string> getLineas(string direA, int cantidad)
        {
            var lasLineas = new List<string>();
            using (StreamReader reader = new StreamReader(direA))
            {
                for (int i = 0; i < cantidad; i++)
                {
                    // Read each line and add it to the list
                    string line = reader.ReadLine();

                    // Break if we reach the end of the file
                    if (line == null)
                        break;

                    lasLineas.Add(line);
                }
            }
            return lasLineas;
        }
    }
}
