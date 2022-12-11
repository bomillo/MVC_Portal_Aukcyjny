using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundTasks.Models
{
    public interface IRateIterator<T>
    {
        public Boolean hasNext();
        public T next();
        public T first();
        public T last();
        public int count();
    }
}
