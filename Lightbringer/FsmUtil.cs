using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using Modding;

// Taken and modified from
// https://raw.githubusercontent.com/KayDeeTee/HK-NGG/master/src/

namespace Lightbringer
{
    internal static class FsmUtil
    {
        // ReSharper disable once InconsistentNaming
        private static readonly FieldInfo FsmStringParamsField;

        static FsmUtil()
        {
            FieldInfo[] fieldInfo = typeof(ActionData).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo t in fieldInfo)
            {
                if (t.Name != "fsmStringParams") continue;
                FsmStringParamsField = t;
                break;
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedMethodReturnValue.Global
        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        public static void RemoveAction(PlayMakerFSM fsm, string stateName, int index)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;

                Array.Resize(ref actions, actions.Length + 1);
                Log(actions[0].GetType().ToString());

                actions.RemoveAt(index);

                t.Actions = actions;
            }
        }

        public static FsmState GetState(PlayMakerFSM fsm, string stateName)
        {
            return (from t in fsm.FsmStates where t.Name == stateName let actions = t.Actions select t).FirstOrDefault();
        }

        public static FsmState CopyState(PlayMakerFSM fsm, string stateName, string newState)
        {
            var state = new FsmState(fsm.GetState(stateName));
            state.Name = newState;

            List<FsmState> fsmStates = fsm.FsmStates.ToList();
            fsmStates.Add(state);
            fsm.Fsm.States = fsmStates.ToArray();

            return state;
        }

        public static FsmStateAction GetAction(PlayMakerFSM fsm, string stateName, int index)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;

                Array.Resize(ref actions, actions.Length + 1);
                Log(actions[index].GetType().ToString());

                return actions[index];
            }

            return null;
        }

        public static T GetAction<T>(PlayMakerFSM fsm, string stateName, int index) where T : FsmStateAction
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;

                Array.Resize(ref actions, actions.Length + 1);
                Log(actions[index].GetType().ToString());

                return actions[index] as T;
            }

            return null;
        }

        public static void AddAction(PlayMakerFSM fsm, string stateName, FsmStateAction action)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                FsmStateAction[] actions = t.Actions;

                Array.Resize(ref actions, actions.Length + 1);
                actions[actions.Length - 1] = action;

                t.Actions = actions;
            }
        }

        public static void ChangeTransition(PlayMakerFSM fsm, string stateName, string eventName, string toState)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                foreach (FsmTransition trans in t.Transitions)
                {
                    if (trans.EventName == eventName) trans.ToState = toState;
                }
            }
        }

        public static void AddTransition(PlayMakerFSM fsm, string stateName, string eventName, string toState)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (t.Name != stateName) continue;
                List<FsmTransition> transitions = t.Transitions.ToList();
                transitions.Add(new FsmTransition
                {
                    FsmEvent = new FsmEvent(eventName),
                    ToState = toState
                });
                t.Transitions = transitions.ToArray();
            }
        }

        public static void RemoveTransitions(PlayMakerFSM fsm, List<string> states, List<string> transitions)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                if (!states.Contains(t.Name)) continue;
                List<FsmTransition> transList = new List<FsmTransition>();
                foreach (FsmTransition trans in t.Transitions)
                {
                    if (!transitions.Contains(trans.ToState))
                        transList.Add(trans);
                    else
                        Log($"Removing {trans.ToState} transition from {t.Name}");
                }

                t.Transitions = transList.ToArray();
            }
        }

        public static void ReplaceStringVariable(PlayMakerFSM fsm, List<string> states, Dictionary<string, string> dict)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                bool found = false;
                if (!states.Contains(t.Name)) continue;
                foreach (FsmString str in (List<FsmString>) FsmStringParamsField.GetValue(t.ActionData))
                {
                    List<FsmString> val = new List<FsmString>();
                    if (dict.ContainsKey(str.Value))
                    {
                        val.Add(dict[str.Value]);
                        found = true;
                    }
                    else
                    {
                        val.Add(str);
                    }

                    if (val.Count > 0) FsmStringParamsField.SetValue(t.ActionData, val);
                }

                if (found) t.LoadActions();
            }
        }

        public static void ReplaceStringVariable(PlayMakerFSM fsm, string state, Dictionary<string, string> dict)
        {
            foreach (FsmState t in fsm.FsmStates)
            {
                bool found = false;
                if (t.Name != state && state != "") continue;
                foreach (FsmString str in (List<FsmString>) FsmStringParamsField.GetValue(t.ActionData))
                {
                    List<FsmString> val = new List<FsmString>();
                    if (dict.ContainsKey(str.Value))
                    {
                        val.Add(dict[str.Value]);
                        found = true;
                    }
                    else
                    {
                        val.Add(str);
                    }

                    if (val.Count > 0) FsmStringParamsField.SetValue(t.ActionData, val);
                }

                if (found) t.LoadActions();
            }
        }

        public static void ReplaceStringVariable(PlayMakerFSM fsm, string state, string src, string dst)
        {
            Log("Replacing FSM Strings");
            foreach (FsmState t in fsm.FsmStates)
            {
                bool found = false;
                if (t.Name != state && state != "") continue;
                Log($"Found FsmState with name \"{t.Name}\" ");
                foreach (FsmString str in (List<FsmString>) FsmStringParamsField.GetValue(t.ActionData))
                {
                    List<FsmString> val = new List<FsmString>();
                    Log($"Found FsmString with value \"{str}\" ");
                    if (str.Value.Contains(src))
                    {
                        val.Add(dst);
                        found = true;
                        Log($"Found FsmString with value \"{str}\", changing to \"{dst}\" ");
                    }
                    else
                    {
                        val.Add(str);
                    }

                    if (val.Count > 0) FsmStringParamsField.SetValue(t.ActionData, val);
                }

                if (found) t.LoadActions();
            }
        }

        private static void Log(string str)
        {
            Logger.LogFine("[FSM UTIL]: " + str);
        }
    }

    ////////////////
    // Extensions //
    ////////////////

    public static class FsmutilExt
    {
        public static void RemoveAction(this PlayMakerFSM fsm, string stateName, int index)
        {
            FsmUtil.RemoveAction(fsm, stateName, index);
        }

        public static FsmState GetState(this PlayMakerFSM fsm, string stateName)
        {
            return FsmUtil.GetState(fsm, stateName);
        }

        public static FsmStateAction GetAction(this PlayMakerFSM fsm, string stateName, int index)
        {
            return FsmUtil.GetAction(fsm, stateName, index);
        }

        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName, int index) where T : FsmStateAction
        {
            return FsmUtil.GetAction<T>(fsm, stateName, index);
        }

        public static FsmState CopyState(this PlayMakerFSM fsm, string stateName, string newState)
        {
            return FsmUtil.CopyState(fsm, stateName, newState);
        }

        public static void AddAction(this PlayMakerFSM fsm, string stateName, FsmStateAction action)
        {
            FsmUtil.AddAction(fsm, stateName, action);
        }

        public static void ChangeTransition(this PlayMakerFSM fsm, string stateName, string eventName, string toState)
        {
            FsmUtil.ChangeTransition(fsm, stateName, eventName, toState);
        }

        public static void AddTransition(this PlayMakerFSM fsm, string stateName, string eventName, string toState)
        {
            FsmUtil.AddTransition(fsm, stateName, eventName, toState);
        }

        public static void RemoveTransitions(this PlayMakerFSM fsm, List<string> states, List<string> transitions)
        {
            FsmUtil.RemoveTransitions(fsm, states, transitions);
        }

        public static void ReplaceStringVariable(this PlayMakerFSM fsm, List<string> states, Dictionary<string, string> dict)
        {
            FsmUtil.ReplaceStringVariable(fsm, states, dict);
        }

        public static void ReplaceStringVariable(this PlayMakerFSM fsm, string state, Dictionary<string, string> dict)
        {
            FsmUtil.ReplaceStringVariable(fsm, state, dict);
        }

        public static void ReplaceStringVariable(this PlayMakerFSM fsm, string state, string src, string dst)
        {
            FsmUtil.ReplaceStringVariable(fsm, state, src, dst);
        }
    }
}