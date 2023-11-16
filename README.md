# comp521-asgn3

# Static Obstacles
### Demo
![static_obstacles](staticObstacles.gif)
### Pathing
![static_obstacles_pathing](staticObstaclesPathing.gif)

# Dynamic Obstacles
### Demo
![dynamic_obstacles](DynamicObstacles.gif)
### Pathing
![dynamic_obstacles_pathing](dynamicObstaclesPathing.gif)


# Algorithmic Approach
- Navigation Mesh
  - The Navigation Mesh is a Grid of Nodes that is laid over the terrain
  - each intersection is a node and each edge is a connection between nodes
  - The Navigation mesh is populated with obstactles every frame. This accounts for dynamic obstacles
  - To populate the Navigation Mesh we take the axis aligned bounding box of each obstacle. We use the corner points of the bound box to index into the navigation mesh.
  - we can then fill in all the nodes that are inside the bounding box as occupied

- Humans
    - Once we have out navigation mesh we can use the A* algorithm to find the shortest path from the start node to the goal node
    - Because of the mesh is discrete and because we populate the mesh only once, the human includes itself as an obstacle which can cause some issues
    - To get around that we do a best first search from where the human is to find unoccupied nodes that are close to where the human currently is. We then use the A* algorithm to find the shortest path from the start node to the goal node

- Chairs
    - The chairs use the same navigation mesh as the humans
    - They also use A* in the same manner
    - the difference being is that they first iterate over every human and pick the one that is closest to them
    - this is the set as the goal node for the A* algorithm


- Note: additionaly we recalculate the obstacles for the mesh twice, once for the humand and once for the chairs. When we do this we add a larger bounding box depending on the type of agent. This is to discourage humans from getting to close to chairs and allowing chairs to get closer to other chairs.
