using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace window_demo
{
    // The following is an example of a combination of Singleton & Observer design pattern . 
    // Here a Logger class is implemented which is Singleton . 
    // Meanwhile all different kinds of logging mechanisms such as FileLogging , Database logging can be implemented as long
    // as they register them selves with the Singleton Logger class . Right now only logging to the File is supported . 
    // In future if needed database support can be easily plugged in . 

    // Create the interface that every logger needs to override . 
    public interface Ilogger
    {
        void dologging(string logmessage);
    }
    // Singleton Logger class 
    class Logger
    {
        private static object checkLock = new object();
        private static Logger log = null;
        private List<Ilogger> observers;

        // Private Constructor to implement the Singleton Pattern
        private Logger()
        {
            //checkLock = new object();
            observers = new List<Ilogger>();
        }

        public static Logger Instance
        {
            get
            {
                // Logger object is never intialised ? In such case only we initialize 
                if (log == null)
                {
                    lock (checkLock)
                    {
                        if (log == null)
                            log = new Logger();
                    }
                }
                return log;
            }
        }
        // Register all logging observers here . Example FileLogging , Database Logging , etc
        public void registerObserver(Ilogger observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
        }
        // Dispatch messages to all the observers waiting for the notification. 
        public void dispatchLogMessage(string message)
        {
            string formatstring = string.Format("{0} - {1}", DateTime.Now.ToString(), message);

            // Dispatch the request to each observer 
            foreach (Ilogger observer in observers)
                observer.dologging(formatstring);

        }
    }

    public class FileLogger : Ilogger
    {
        private string filename;
        private StreamWriter handle;
        private FileStream fs;

        public string getName
        {
            get
            {
                return filename;
            }
        }
        public FileLogger(string fname)
        {
            filename = fname;
        }
        public void Close()
        {
            handle.Close();
        }
        public void dologging(string message)
        {

            using (StreamWriter handle = new StreamWriter(filename, true))
            {
                handle.WriteLine(message);
            }
        }
    }
}
