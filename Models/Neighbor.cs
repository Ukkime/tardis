using System.Collections.Generic;
using tardis.Models;

public class Neighbor
{
    public string Name { get; set; }
    public int NodeCount { get; set; }
    public string Id { get; set; }
    public List<NeighborNode> NeighborNodes { get; set; }
}