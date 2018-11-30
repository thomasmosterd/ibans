using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;
namespace ConsoleApp5
{
    class Program
    {
        static int ondergrens, bovengrens, p, modulus, Atrue, verschil, locktype, modus;
        static string hash, iban;
        static bool found = false;
        static object slot = new Object();
        static Tas_Lock slots = new Tas_Lock();
        static SHA1 sha = SHA1.Create();
        public static void Main(string[] args)
        {
            //tel:  0 274856170 274856190 11 4 0 0
            //lijst:  0 274856170 274856190 11 4 1 0
            //hash: 0 274856170 274856190 11 4 2 c736ca9048d0967a27ec3833832f7ffb571ebd2f
            //test 1: 0 139483 139500 11 1 0 0
            DateTime startime = DateTime.Now;
            string[] input = new string[7];
            input = Console.ReadLine().Split();
            locktype = Int32.Parse(input[0]);
            ondergrens = Int32.Parse(input[1]);
            bovengrens = Int32.Parse(input[2]);
            modulus = Int32.Parse(input[3]);
            p = Int32.Parse(input[4]);
            modus = Int32.Parse(input[5]);
            hash = input[6];
            verschil = (bovengrens - ondergrens) / p;
            Atrue = 0;
            Thread[] ts = new Thread[p];
            switch (modus)
            {
                case 0:
                    for (int t = 0; t < p; t++)
                        ts[t] = new Thread(standaardshit);
                    break;
                case 1:
                    for (int t = 0; t < p; t++)
                        ts[t] = new Thread(standaardshit);
                    break;
                case 2:
                    for (int t = 0; t < p; t++)
                        ts[t] = new Thread(Hashmodus);
                    break;

            }
            for (int k = 0; k < p; k++)
            {
                ts[k].Start(k);
            }
            DateTime punt1 = DateTime.Now;
            for (int j = 0; j < p; j++)
            {
                ts[j].Join();
            }
            DateTime punt2 = DateTime.Now;
            //Console.WriteLine(startime + "  " + punt1 + "  " + punt2);
            if (modus == 0) Console.WriteLine(Atrue);
            if (modus == 2 && found == false) Console.WriteLine(-1);
            Console.ReadLine();
        }
        public static bool hashmod(int ct)
        {
            byte[] hashArray = sha.ComputeHash(Encoding.ASCII.GetBytes(ct.ToString()));
            string newHash = "";
            for (int hashPos = 0; hashPos < hashArray.Length; hashPos++)
                newHash += hashArray[hashPos].ToString("x2");
            if (newHash == hash)
            {
                return true;
            }
            return false;
        }
        public static void standaardshit(object mt)
        {
            int van = ondergrens + verschil * (int)mt;
            int tot = ondergrens + ((int)mt + 1) * verschil;
            for (int bt = van; bt < tot; bt++)
            {
                string nummer = bt.ToString();
                int[] lol = new int[nummer.Length];
                int h = 0;
                foreach (char s in nummer)
                {
                    lol[h] = s;
                    h++;
                }
                int total = 0;
                int i = 1;
                Array.Reverse(lol);
                foreach (int s in lol)
                {
                    total += (s - 48) * i;
                    i++;
                }
                if (total % modulus == 0)
                {
                    switch (modus)
                    {
                        case (0):
                            telmodus();
                            break;
                        case (1):
                            lijstmodus(bt);
                            break;
                    }
                }
            }
        }
        public static void telmodus()
        {
            if (locktype == 0)
            {
                slots.Locken();
                Atrue++;
                slots.Unlocken();
            }
            else
            {
                lock (slot)
                {
                    Atrue++;
                }
            }
        }
        public static void lijstmodus(int ct)
        {
            if (locktype == 0)
            {
                slots.Locken();
                Atrue++;
                Console.WriteLine(Atrue + " " + ct);
                slots.Unlocken();
            }
            else
            {
                lock (slot)
                {
                    Atrue++;
                    Console.WriteLine(Atrue + " " + ct);
                }
            }
        }
        public static void Hashmodus(object mt)
        {
            int van = ondergrens + verschil * (int)mt;
            int tot = ondergrens + ((int)mt + 1) * verschil;
            for (int bt = van; bt < tot; bt++)
            {
                if (found)
                    Thread.CurrentThread.Abort();
                string nummer = bt.ToString();
                int[] lol = new int[nummer.Length];
                int h = 0;
                foreach (char s in nummer)
                {
                    lol[h] = s;
                    h++;
                }
                int total = 0;
                int i = 1;
                Array.Reverse(lol);
                foreach (int s in lol)
                {
                    total += (s - 48) * i;
                    i++;
                }
                if (bt % modulus == 0)
                {
                    if (locktype == 1)
                    {
                        if (hashmod(bt))
                        {
                            lock (slot)
                            {
                                found = true;
                                Console.WriteLine(bt);
                                Thread.CurrentThread.Abort();
                            }
                        }
                    }
                    else
                    {
                        if (hashmod(bt))
                        {
                            slots.Locken();
                            found = true;
                            Console.WriteLine(bt);
                            slots.Unlocken();
                            Thread.CurrentThread.Abort();
                        }
                    }
                }
                //if (total % modulus == 0)
                //{
                //    if (hashmod(bt))
                //    {
                //        if (locktype == 0)
                //        {
                //            slots.Locken();
                //            found = true;
                //            Console.WriteLine(bt);
                //            slots.Unlocken();
                //        }
                //        if (locktype == 1)
                //        {
                //            lock (slot)
                //            {
                //                found = true;
                //                Console.WriteLine(bt);
                //                Thread.CurrentThread.Abort();
                //            }
                //        }
                //    }
                //}
            }
        }
    }
    public class Tas_Lock
    {
        int S = 0;
        public void Locken()
        {
            while (Interlocked.CompareExchange(ref S, 1, 0) != 0) { }
        }
        public void Unlocken()
        {
            Interlocked.Exchange(ref S, 0);
        }
    }
}
