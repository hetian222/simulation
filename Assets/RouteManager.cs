using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Route
{
    public List<Transform> points;
}

public class RouteManager : MonoBehaviour
{
    public List<Route> routes;

    public List<Transform> GetRouteByIndex(int index)
    {
        if (index >= 0 && index < routes.Count)
        {
            return routes[index].points;
        }
        Debug.LogWarning($"Route index {index} Խ�磬���ص�0��·��");
        return routes[0].points;
    }
}
