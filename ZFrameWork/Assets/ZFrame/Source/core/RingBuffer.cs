using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;


namespace sgaf.core
{
    public class RingBuffer<T>
    {
        protected List<T> buffer;
        int readpos;
        int writepos;
        int capacity;
        int flag;
        static T defval = default(T);

        public int Flag {
            get { return flag; }
            set { flag = value;}
        }

        public int Capacity {
            get { return capacity; }
            set { capacity = value; }
        }

        public RingBuffer (int capacity)
        {
            flag = 0;
            Reset (capacity);
        }

        public void Reset (int capacity)
        {
            buffer = new List<T> (capacity);
            writepos = -1;
            readpos = -1;
            this.capacity = capacity;
        }

        public bool Push(T data)
        {
            int nextpos = writepos + 1;
            if (nextpos == capacity) {
                nextpos = 0;
            }
            if (nextpos == readpos) {
                return false;    
            }
            if (writepos < 0) {
                writepos = 0;
				readpos = 0;
                nextpos = 1;
            }
            if (buffer.Count < buffer.Capacity) {
                buffer.Insert(writepos, data);
            } else {
                buffer[writepos] = data;
            }
            writepos = nextpos;
            return true;
        }

        public bool Pop(ref T data)
        { 
            if (readpos == writepos) {
                return false;
			}

            data = buffer [readpos];
            buffer[readpos] = defval;
            if (readpos + 1 < capacity) {
                ++readpos;
            } else {
                readpos = 0;
            }
            return true;
        }

        public bool GetTop(ref T data)
        {
            if (readpos == writepos) {
                return false;
            }
            data = buffer [readpos];
            return true;
        }

        public bool IsEmpty()
        {
            if (writepos != readpos) {
                return false;
            }
            return true;
        }
    }
}

