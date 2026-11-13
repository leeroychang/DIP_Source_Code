using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    private void Awake() {
        if (Instance!=null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Type[] types = Assembly.GetExecutingAssembly().GetTypes().Where(T => T.IsSubclassOf(typeof(GameEvent)) && !T.IsAbstract).ToArray();

        foreach (Type type in types)
        {
            preSubs[type] = new();
            postSubs[type] = new();
        }
        
    }

    private void OnDestroy() {
        if (Instance==this) Instance = null;
    }



    private Queue<GameEvent> events = new();
    private Dictionary<Type,List<Reaction>> preSubs = new();
    private Dictionary<Type,List<Reaction>> postSubs = new();
    private List<Reaction> removedSubs = new();

    public static void Queue(GameEvent gameEvent)
    {
        if (Instance!=null) Instance.events.Enqueue(gameEvent);
        else Debug.LogWarning("No Event Manager Instance active.");
    }

    public static void Subscribe(Type eventType, Reaction reaction, Reaction.ReactionTiming timing)
    {
        if (Instance==null) return;
        if (timing==Reaction.ReactionTiming.PRE) Instance.preSubs[eventType].Add(reaction);
        else Instance.postSubs[eventType].Add(reaction);
        reaction.Subscribe();
    }

    public static void Unsubscribe(Type eventType, Reaction reaction, Reaction.ReactionTiming timing)
    {
        if (Instance==null) return;
        if (timing==Reaction.ReactionTiming.PRE) Instance.preSubs[eventType].Remove(reaction);
        else Instance.postSubs[eventType].Remove(reaction);
        Instance.removedSubs.Add(reaction);
        reaction.Unsubscribe();
    }

    void Update()
    {
        if (Time.timeScale==0) return; //Prevent Actions while paused

        while (events.Count!=0)
        {
            GameEvent gameEvent = events.Dequeue();
            Perform(gameEvent);
        }
    }

    private void Perform(GameEvent gameEvent)
    {
        if (gameEvent.target!=null && gameEvent.target.IsDown()) return;
        List<Reaction> reactions;
        reactions = preSubs[gameEvent.GetType()];
        foreach (Reaction reaction in reactions.ToList()) if (!removedSubs.Contains(reaction)) reaction.Call(gameEvent);
        removedSubs.Clear();

        gameEvent.Call();

        reactions = postSubs[gameEvent.GetType()];
        foreach (Reaction reaction in reactions.ToList()) if (!removedSubs.Contains(reaction)) reaction.Call(gameEvent);
        removedSubs.Clear();
    }




    

}
