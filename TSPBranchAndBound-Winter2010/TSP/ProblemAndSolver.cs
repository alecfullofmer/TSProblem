using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace TSP
{

   public class ProblemAndSolver
    {

        private class TSPSolution
        {

            public ArrayList
                Route;
            

            
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// you are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your node data structure and search algorithm. 
            /// </summary>
            

            public TSPSolution(ArrayList iroute)
            {
                
                Route = new ArrayList(iroute);
                
            }

          

            

           

            

            /// <summary>
            ///  compute the cost of the current route.  does not check that the route is complete, btw.
            /// assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here; 
                double cost = 0D;
                
                for (x = 0; x < Route.Count-1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }
                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost; 
            }
        }

        #region private members
        private const int DEFAULT_SIZE = 25;
        
        private const int CITY_ICON_SIZE = 5;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf;

        private double bestSoFar;

       private double solutionTime;

       System.Diagnostics.Stopwatch timer;

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;
        #endregion

        #region public members.
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        public const int DEFAULT_SEED = -1;

        #region Constructors
        public ProblemAndSolver()
        {
            initialize(DEFAULT_SEED, DEFAULT_SIZE);
        }

        public ProblemAndSolver(int seed)
        {
            initialize(seed, DEFAULT_SIZE);
        }

        public ProblemAndSolver(int seed, int size)
        {
            initialize(seed, size);
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// reset the problem instance. 
        /// </summary>
        private void resetData()
        {
            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null; 

            for (int i = 0; i < _size; i++)
                Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.LightGray,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        private void initialize(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            if (seed != DEFAULT_SEED)
                this.rnd = new Random(seed);
            else
                this.rnd = new Random();
            this.resetData();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size)
        {
            this._size = size;
            resetData(); 
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-15F;
            Font labelFont = new Font("Arial", 10);

            g.DrawString("n(c) means this node is the nth node in the current solution and incurs cost c to travel to the next node.", labelFont, cityBrushStartStyle, new PointF(0F, 0F)); 

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        ///  solve the problem.  This is the entry point for the solver when the run button is clicked
        /// right now it just picks a simple solution. 
        /// </summary>
        public void solveProblem()
        {
            timer = new System.Diagnostics.Stopwatch();
            bool timeUp = false;
            timer.Start();
            /*
            // this is the trivial solution. 
            int x;
            Route = new ArrayList();
            
            for (x = 0; x < Cities.Length; x++)
            {
                Route.Add( Cities[Cities.Length - x -1]);
            }
            // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
            bssf = new TSPSolution(Route);
            // update the cost of the tour.
             */

            
            double[,] distance_matrix = new double[_size, _size];

            for (int i = 0; i < _size; i++)
            {
                for (int j = i; j < _size; j++)
                {
                    if (j == i)
                    {
                        distance_matrix[j, j] = Double.PositiveInfinity;
                    }
                    else
                    {
                        City here = Cities[i] as City;
                        City there = Cities[j] as City;
                        distance_matrix[i, j] = distance_matrix[j, i] = here.costToGetTo(there);

                    }
                }
            }

            initBSSF(distance_matrix);
            State initial = init(distance_matrix, _size);
            NeatQueue agenda = new NeatQueue();
            agenda.add(initial);
            agenda.pushChildren();
            while (agenda.getUpToDateQueue().Count > 0)
            {
                State workingState = agenda.pop();
                if (workingState.bound < bestSoFar)
                {
                    List<State> children = getChildren(workingState);
                    foreach (State child in children)
                    {
                        if (timer.Elapsed.Minutes > 0)
                        {
                            timer.Stop();
                            timeUp = true;
                            break;
                        }
                        else if (MeetsCriterion(child))
                        {
                            setBSSF(child);
                        }
                        else
                            agenda.add(child);
                    }
                    agenda.pushChildren();
                }
                if (timeUp)
                {
                    break;
                }
                    
            }

            Program.MainForm.tbCostOfTour.Text = " " + bestSoFar;
            Program.MainForm.tbElapsedTime.Text = " " + solutionTime;
            // do a refresh. 
            Program.MainForm.Invalidate();
        }

        private class State : IComparable
        {
            public double[,] matrix;
            public double bound;
            public List<int> settledCities;

            public State(double b, List<int> ss, double[,] m)
            {
                matrix = m;
                bound = b;
                settledCities = ss;
            }

            public int CompareTo(Object obj)
            {
                if (obj == null)
                    return 1;

                State otherState = (State)obj;

                if (otherState != null)
                    return this.bound.CompareTo(otherState.bound);
                else
                    throw new ArgumentException("YO, this aint a State, B.");
            }
                
        }

        private void setBSSF(State child)
        {
            double tempTime = timer.Elapsed.Seconds;
            ArrayList cities = new ArrayList();
            for (int j = 0; j < _size; ++j)
            {
                cities.Add(Cities[child.settledCities[j]]);
            }
            TSPSolution newRoute = new TSPSolution(cities);
            if (newRoute.costOfRoute() < bestSoFar)
            {
                bssf = newRoute;
                bestSoFar = newRoute.costOfRoute();
                solutionTime = tempTime;
            }
            
        }
        private class NeatQueue
        {
            private List<State> forReal = new List<State>();
            private List<State> children = new List<State>();

            public List<State> getUpToDateQueue()
            {
                return forReal;
            }

            public void add(State child)
            {
                children.Add(child);
            }

            public void pushChildren()
            {
                foreach (State s in forReal)
                {
                    children.Add(s);
                }
                forReal = children;
                children = new List<State>();
            }

            public State pop()
            {
                State s = forReal[0];
                forReal.RemoveRange(0, 1);
                return s;
            }
        }



        private State init(double[,] matrix, int size)
        {
            double totalLowerBound = 0;
            for (int i = 0; i < size; i++)
            {
                double min = Double.PositiveInfinity;
                for (int j = 0; j < size; j++)
                {
                    if (matrix[i, j] < min)
                    {
                        min = matrix[i, j];
                    }
                }
                for (int j = 0; j < size; j++)
                {
                    matrix[i, j] -= min;
                }
                totalLowerBound += min;
            }

            for (int i = 0; i < size; i++)
            {
                double min = Double.PositiveInfinity;
                for (int j = 0; j < size; j++)
                {
                    if (matrix[j, i] < min)
                    {
                        min = matrix[j, i];
                    }
                }
                for (int j = 0; j < size; j++)
                {
                    matrix[j, i] -= min;
                }
                totalLowerBound += min;
            }

            State state = new State(totalLowerBound, new List<int>(), matrix);
            return state;
        }

        private void initBSSF(double[,] matrix)
        {
            bestSoFar = 0;
            List<int> visited = new List<int>();
            ArrayList cities = new ArrayList();
            int next = 0;
            visited.Add(next);
            cities.Add(Cities[next]);
            while (visited.Count < _size)
            {
                double min = Double.PositiveInfinity;
                int tempNext = -1;
                for (int j = 0; j < _size; j++)
                {
                    if (matrix[next, j] < min && !visited.Contains(j))
                    {
                        min = matrix[next, j];
                        tempNext = j;
                    }

                }
                next = tempNext;
                visited.Add(next);
                cities.Add(Cities[next]);
                bestSoFar += min;
            }

            bestSoFar += matrix[0, visited[_size -1]];
            bssf = new TSPSolution(cities);
            solutionTime = timer.Elapsed.Seconds;

        }


        private List<State> getChildren(State current)
        {
            List<State> children = new List<State>();
            int lastVisited = -1;
            if (current.settledCities.Count > 0) 
                lastVisited = current.settledCities[current.settledCities.Count - 1];
            for (int i = 0; i < Cities.Length; i++)
            {
                if (!current.settledCities.Contains(i))
                {
                    double child_lb = current.bound;
                    if (lastVisited != -1)
                        child_lb += current.matrix[i, lastVisited];
                    if (bestSoFar > child_lb)
                    {
                        List<int> settled = new List<int>(current.settledCities);
                        settled.Add(i);
                        double[,] childMatrix = (double[,])current.matrix.Clone();
                        for (int j = 0; j < _size; j++)
                        {
                            childMatrix[i, j] = Double.PositiveInfinity;
                            if(lastVisited != -1)
                                childMatrix[j, lastVisited] = Double.PositiveInfinity;
                        }
                        children.Add(new State(child_lb, settled, childMatrix));
                    }
                }
            }

            children.Sort();
            return children;
        }

        private bool MeetsCriterion(State child)
        {
            if (child.settledCities.Count < _size)
                return false;

            List<int> sortedSettled = new List<int> (child.settledCities);
            sortedSettled.Sort();

            for (int i = 0; i <_size; i++)
            {
                if (i != sortedSettled[i])
                    return false;
            }

            return true;
        }
        #endregion
    }
}
