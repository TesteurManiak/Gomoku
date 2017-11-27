using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pbrain_Gromoku
{
    abstract class GomocupInterface
    {
        /* information about a game - you should use these variables */
        public int width, height; /* the board size */
        public int info_timeout_turn = 30000; /* time for one turn in milliseconds */
        public int info_timeout_match = 1000000000; /* total time for a game */
        public int info_time_left = 1000000000; /* left time for a game */
        public int info_max_memory = 0; /* maximum memory in bytes, zero if unlimited */
        public int info_game_type = 1; /* 0:human opponent, 1:AI opponent, 2:tournament, 3:network tournament */
        public bool info_exact5 = false; /* false:five or more stones win, true:exactly five stones win */
        public bool info_renju = false; /* false:gomoku, true:renju */
        public bool info_continuous = false; /* false:single game, true:continuous */
        public int terminate; /* return from brain_turn when terminate>0 */
        public int start_time; /* tick count at the beginning of turn */
        public string dataFolder; /* folder for persistent files, can be null */

        abstract public string  brain_about { get; }
        abstract public void brain_init();
        abstract public void brain_restart(); /* delete old board, create new board, call Console.WriteLine("OK"); */
        abstract public void brain_turn(); /* choose your move and call do_mymove(x,y); 0<=x<width, 0<=y<height */
        abstract public void brain_my(int x, int y); /* put your move to the board */

        private string cmd;
        private AutoResetEvent event1;
        private ManualResetEvent event2;

        protected void do_mymove(int x, int y)
        {
            brain_my(x, y);
            Console.WriteLine("{0},{1}", x, y);
        }

        private void threadLoop()
        {
            for (; ; )
            {
                event1.WaitOne();
                brain_turn();
                event2.Set();
            }
        }

        public void main()
        {
            try
            {
                int dummy = Console.WindowHeight;
                Console.WriteLine("MESSAGE Gomoku AI should not be started directly. Please install gomoku manager (http://sourceforge.net/projects/piskvork). Then enter path to this exe file in players settings.");
            }
            catch (System.IO.IOException)
            {}
            event1 = new AutoResetEvent(false);
            new Thread(threadLoop).Start();
        }
    }
    class GomocupEngine : GomocupInterface
    {
        const int MAX_BOARD = 100;
        int[,] board = new int[MAX_BOARD, MAX_BOARD];
        Random rand = new Random();

        public override string brain_about
        {
            get
            {
                return "name=\"Random\", author=\"Petr Lastovicka\", version=\"1.1\", country=\"Czech Republic\", www=\"http://petr.lastovicka.sweb.cz\"";
            }
        }

        public override void brain_init()
        {
            if (width < 5 || height < 5)
            {
                Console.WriteLine("ERROR size of the board");
                return;
            }
            if (width > MAX_BOARD || height > MAX_BOARD)
            {
                Console.WriteLine("ERROR Maximal board size is " + MAX_BOARD);
                return;
            }
            Console.WriteLine("OK");
        }

        public override void brain_restart()
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    board[x, y] = 0;

            Console.WriteLine("OK");
        }

        private bool isFree(int x, int y)
        {
            return x >= 0 && y >= 0 && x < width && y < height && board[x, y] == 0;
        }

        public override void brain_my(int x, int y)
        {
            if (isFree(x, y))
            {
                board[x, y] = 1;
            }
            else
            {
                Console.WriteLine("ERROR my move [{0},{1}]", x, y);
            }
        }

        public override void brain_turn()
        {
            int x, y, i;

            i = -1;
            do
            {
                x = rand.Next(width);
                y = rand.Next(height);
                i++;
                if (terminate != 0) return;
            } while (!isFree(x, y));

            if (i > 1) Console.WriteLine("DEBUG {0} coordinates didn't hit an empty field", i);
            do_mymove(x, y);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            new GomocupEngine().main();
        }
    }
}
