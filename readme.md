# Hanna Roads System

This is a Unity road system, with that you can build amazing roads with a lot a features!

## How to use

### Creating a simple road system

- Go to top menu select Hanna Roads and then "Create Road System".

![Create Road System](imgs/hanna_road_create_road.png)


- To start building a road select HannaRoad Object created in the hierarchy.

![hanna roads inspector](imgs/hanna_road_in_hierarchy.png)

- When you hold Shift you will see a little wire cube.

![hanna roads inspector](imgs/wire_cube.png)
- Click with left mouse button to create and keep holding shift to choose where to go with the end of the road.

![hanna roads inspector](imgs/placing_road.gif)

- To confirm the creation of the road click with left mouse button again and voilà a new amazing road! If want to cancel, just release shift key.

![hanna roads inspector](imgs/new_road_created.png)

### Controlling the road

When you create a new road you will see circles with a green line and a white cube towers, the roads are Bézier curves, the circles are the controllers of the Bézier curve, when you move then they will change the shape of the road, with that you can create a nice curves without need to create more roads. If you need to move the road use the towers to change shape of the road.

![hanna roads inspector](imgs/controlling_the_road.gif)

You can delete the road freely, when you delete the road they will disconnect from other roads, remember, if you need to delete a road, delete the segments, deleting reference points, control points or important objects can cause erros, if you accidentally delete some of this objects, delete the segment and create another, you can mark other road as active and keep making more roads.

![hanna roads inspector](imgs/deleting_roads.gif)


### How connection works

Hanna roads can create connected roads and disconnected roads, after creating your first road, if you try to create another road the start of the road will be the end of the previous one.

![hanna roads inspector](imgs/road_connection.gif)

If you don´t need that before you click with the left mouse button for the first time you can hold alt key when holding shift, the wire cube will become green and you will be able to create a disconnected road.

![hanna roads inspector](imgs/road_disconnected.gif)

### Active Segment

Hanna roads have segments and intersections, when you create a road you are creating a segment, when a segment is created they become the active segment, you can verify looking to Hanna Road game object and see in the field "active segment", you will see an active intersection too(we will talk about that later in this document), when you create a new road that will be connected to another road Hanna road will get the active segment to connect the roads, 

![hanna roads inspector](imgs/hanna_road_inspec.png)


if you need to start a new road from another segment you can simple select the segment you need and click "Set as Active", when you return to Hanna roads object you can see in the active segment field the road you have marked as active.

![hanna roads inspector](imgs/set_active.png)

### Intersections

Let´s talk about intersections, you have multiple ways to create an intersection, let´s go to the easy way. Select your Hanna Road object, you will see a "Change mode" button, change mode to intersection(you need to have at least one road in the scene to change mode). you will see a "Road mode" change to intersection.
![hanna roads inspector](imgs/change_mode.gif)




After that you have two options, create at the start of the road, or create at the end of the road, you can hold shift and press E to create at the end or S to create at the start, if the road is connected with another road  at the end will not be possible to create at the end and vice-versa

![creating at end](imgs/creating_Intersection_at_end.gif)

![creating at start](imgs/creating_Intersection_at_start.gif)

After that if you change mode to intersection and try to create another connected road, you will see the start of the new road be a point of the intersection, if you scroll your mouse you will see the road change from one intersection connection to another.
![creating at start](imgs/creating_road_connected_to_intersection.gif)
 
#### **Warning**

**_When you create the road that start from intersection you probably will see two roads connected to same connection, just scroll the mouse to change to another connection point._**

If you need to change connection you can select the white cube tower connected to intersection that is called Reference point, in that you can can change intersection position in a slider.
 

### Active intersections

As we have active segments we have active intersections too, as you see when we have an active intersection and using intersection mode, the road starts from that active intersection, but you can do more things.


If you select a reference point, and press shift + e the reference point will be attached to the active intersection and connecting the road with the intersection

![connecting to intersection](imgs/connecting_to_intersection.gif)

If you want to disconnect, click on the "Intersection connected" field and press delete key to remove reference, now you can move reference point freely, you can drag and drop intersection object to intersection field in the reference point too.

![Disconnecting from intersection](imgs/disconnecting_from_intersection.gif)


When you click on the reference point you can se a button called "Add Intersection", pressing this button will create a intersection at reference point position.
![Creating road at reference point](imgs/creating_intersection_at_reference_point.gif)



### Connecting separated roads and making loop roads

You probably will need to connect two roads, or create road loops, to do that select two reference points and this two need to be a start reference point and a end reference point, in other words the start and the end of the roads, to know that you can select the reference point and see the segment type, or looking for colors of the towers, if is white is the start if is little bit darker is the end. After selecting this two click "Connect", another road will be created to connect the roads.
![Disconnecting from intersection](imgs/connecting_roads.gif)



## Changing road and intersection shapes and geometry

With Hanna Roads you can make cool things, but let start from the basics.


### Road Segment geometry

When you select a segment you will find some settings:





* Width : Change the width of the road
* Detail Level : Change the number of cuts(edges) of the road, more cuts, more detail
* Horizontal detail leve: Add cuts horizontally to the road creating extra detail, you will need that to create custom shapes.

This is the basic, but only that is boring, let´s make cool stuff!

After basic settings you will see 3 important settings to custom shapes.

  * Width smoothness curve
  * Height smoothness curve
  * Vertical profile multiplayer




If you change vertical profile multiplayer you will see the road starting bend up.



This happens because of the height smoothness curve, if you see the curve they are lower at start and high at end, Hanna roads create a shape based on this curve, if you change the curve, the shape of the road will change, with that you can create crazy shapes, you can adjust using vertical multiplier, cool right?