using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JsonParser
{
    public class Automaton<T> where T : struct, IConvertible
    {
       

        private struct StateTransition<Ts>
        {
            public Ts State;
            public Expression<Func<char, bool>> Input;
            public object Push;
            public object Pop;
           
        };
                
        private Stack<Object> MainStack = new Stack<object>();
        private Dictionary<T, List<StateTransition<T>>> Transitions = new Dictionary<T, List<StateTransition<T>>>();
        private Dictionary<T, bool> HasEmptyTransitions = new Dictionary<T, bool>();
        private List<T> FinalStates = new List<T>();
        public T State;
        private T InitialState;
        private bool Started = false;
        private List<T> TempWhen = new List<T>();

        public event EventHandler OnStackPush;
        public event EventHandler OnStackPop;
        public virtual void DoOnStackPush(EventArgs e, Object o) { if (OnStackPush != null)  OnStackPush(o, e); }
        public virtual void DoOnStackPop(EventArgs e, Object o) { if (OnStackPop != null)  OnStackPop(o, e); }

        public Automaton()
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");



        }

        public Automaton<T> StartIn(T state)
        {
            InitialState = state;
            return this;
        }

        public Automaton<T> EndIn(params T[] states)
        {
            FinalStates.AddRange(states);
            return this;
        }

        public Automaton<T> When(params T[] states)
        {
            TempWhen.Clear();
            foreach (var state in states)
            {
                if (Transitions.ContainsKey(state))
                    throw new ApplicationException("State already defined");

                Transitions.Add(state, new List<StateTransition<T>>());
                TempWhen.Add(state);
            }
           

           
           
            return this;
        }

        public Automaton<T> On(Expression<Func<char, bool>> input, T newState, Object Pop = null, Object Push = null)
        {

            var trasition = new StateTransition<T>();
            trasition.Input = input;
            trasition.Pop = Pop;
            trasition.Push = Push;
            trasition.State = newState;

            foreach (var state in TempWhen)
            {
                Transitions[state].Add(trasition);
                HasEmptyTransitions[state] = true;
            }
            return this;
        }

        public Automaton<T> On(char input, T newState, Object Pop = null, Object Push = null)
        {
            return On(e => e.Equals(input), newState, Pop, Push);
        }

        public Automaton<T> On(string input, T newState, Object Pop = null, Object Push = null)
        {
            foreach (var c in input)
                On(c, newState, Pop, Push);
            return this;
        }

        public Automaton<T> On(Regex regExp, T newState, Object Pop = null, Object Push = null)
        {
            return On(e => regExp.IsMatch(e.ToString()), newState, Pop, Push);
        }

        public Automaton<T> On(T newState, Object Pop = null, Object Push = null)
        {

            var trasition = new StateTransition<T>();
            trasition.Input = null;
            trasition.Pop = Pop;
            trasition.Push = Push;
            trasition.State = newState;

            foreach (var state in TempWhen)
            {
                Transitions[state].Add(trasition);
                HasEmptyTransitions[state] = true;
            }
            
            return this;
        }


        public void Reset()
        {
            MainStack.Clear();
            Started = false;
        }

        public bool Valid()
        {
            return FinalStates.Contains(State) && MainStack.Count == 0;
        }

        public bool Read(string input)
        {
            var ret = true;
            foreach (var c in input)
            {
                if (!(ret = Read(c)))
                    break;
            }

            return ret;
        }
        public bool Read(char input)
        {

            if (!Started)
            {
                State = InitialState;
                Started = true;
            }


            if (!Transitions.ContainsKey(State))
                return false;

            var stateTrans = Transitions[State];
             var ret = false;


            foreach (var t in stateTrans)
            {

                var validStack = !(t.Pop == null);
                var match = t.Input!=null && t.Input.Compile()(input);

                var noStack = true;
                if (t.Push != null && t.Pop != null)
                    noStack = !(t.Pop.Equals(t.Push));


                if (match)
                {
                    if (validStack)
                    {
                        if (MainStack.Count > 0 && MainStack.Peek().Equals(t.Pop))
                        {
                            if (noStack)
                                Pop();
                            State = t.State;
                            ret = true;
                        }
                        else
                        { continue; }
                    }
                    else
                    {
                        this.State = t.State;
                        ret = true;
                    }


                    if (!(t.Push == null) && noStack)
                        Push(t.Push);

                }


                if (ret)
                    break;
            }


            if (!ret) // invalida resultados da maquina
                MainStack.Push(null);

            if (HasEmptyTransitions[State])
                DoEmptyTransitions();

            return ret;
        }

        private void DoEmptyTransitions()
        {
            // valid new empty transatcions

            if (!Transitions.ContainsKey(State))
                return;


            if (MainStack.Count > 0 && MainStack.Peek() == null)
                return;

            var stateTrans = Transitions[State];
            var changed = false;

            foreach (var t in stateTrans) {
                var validStack = !(t.Pop == null);
                var match = t.Input == null;
                var noStack = true;
                if (t.Push != null && t.Pop != null)
                    noStack = !(t.Pop.Equals(t.Push));

                if (match)
                {
                    if (validStack)
                    {
                        if (MainStack.Count > 0 && MainStack.Peek().Equals(t.Pop))
                        {
                            if (noStack)
                                Pop();

                            State = t.State;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        this.State = t.State;
                    }

                    if (!(t.Push == null) && noStack)
                        Push(t.Push);

                    changed = true;
                    break;
                }

            }

            if (changed && HasEmptyTransitions[State])
                DoEmptyTransitions();


        }


        public object Peek() { return MainStack.Count > 0 ? MainStack.Peek() : null; }
        public object Pop() {           
            DoOnStackPop(EventArgs.Empty, this);
            return MainStack.Pop(); 
        }
        public void Push(object item) {
            MainStack.Push(item);
            DoOnStackPush(EventArgs.Empty, this);
        }
        public int StackLenght() { return MainStack.Count; }

    }


}
