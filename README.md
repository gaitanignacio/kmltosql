KML2SQL v2
=====

Overview
---
KML2SQL is a utility that uploads a KML file to a Microsoft SQL database, storing the placemarks in geography or geometry objects. It comes in two flavors:


1. For regular people, [Download the Desktop Utility here.](http://pharylon.github.io/KML2SQL/download.html)
---
2. For Developers, the backend logic is available as [a Nuget Package](https://www.nuget.org/packages/Kml2Sql/).
---
3. Either way, [the documentation is here](http://pharylon.github.io/KML2SQL/).
---

What's New:
-----
**05/03/2016 - v 2.1**
* Added the "Name" property of placemarks as a column.

**03/31/2016 - v 2.0**
* Better ability to fix broken polygons.
* Improved UI layout.
* Polygons can have inner open spaces. Think "donut holes." (*Special thanks to Apathy Dude!*)
* Ability to export a SQL Script instead of uploading objects directly.
* Vastly improved backend logic for Nuget Package separation.


**03/05/2015 - v 1.4**
* Ability to use Windows Integrated Security (thanks mirhagk!)

**06/05/2014 - v 1.3**
* Persistence. It will remember your username and server between uses (thanks Simon Timms!)
* Improved error reporting to user.
* If one of the placemarks can't be uploaded, the entire file won't fail. Placemarks that don't upload are noted in the error log.
* Just a bunch of behind-the-scenes cleanup to make it slightly less ugly.

**11/10/2013 - v 1.2**
* Better error reporting when something goes wrong.
* Made it more clear that a new table will be created, and existing tables will be overwritten.

**06/02/2013 - v 1.1** - A big update that has significantly reworked the backend and allows for the following:
* **Improved Security!** I think it's safe against SQL injection, but you should still be careful.
* **Placemark Data!** SimpleData and Data types are now uploaded as additional columns. Note that since Data entries are untyped and SimpleData schemas are unreliable, all data is uploaded into Varchar(max) columns. Oh, and nulls are allowed in all columns except ID and geometry/geography. So you end up with a really inefficient database, but converting them all to ints, floats, or whatever is just a few SQL commands away, right?
* **Improved Expandability!** It should be easier to add support for mySQL and the like in the future.

To-Do List:
-----

* Provide support for polygons with lines that intersect (in real life they shouldn't, but KML lets you draw polygons like that. Code would need to be able to detect that and fix, which would be fairly hard).
* Provide support for KMZ files (but really, those are just zipped KML files, so if you have one of those, just unzip it for now).

If you have any questions, feel free to post issues here on GitHub, email me at zach(at)zachshuford(dotcom) or tweet me @pharylon. If something isn't working, let me know! If you really need some feature bad, let me know! The amount of work I put into this project and number of features I add is directly proportional to how many people ask for them.

Lastly, a big "Thank You!" to [SharpKML](http://sharpkml.codeplex.com/) without which this project would be a lot more work and [VectorLady](http://vectorlady.com/) who authored the icon, which is released under the Creative Commons License 3.0 Attribution.

Finally, KML2SQL itself is licensed under the [BSD 2 Clause License](https://github.com/Pharylon/KML2SQL/blob/master/License.txt).
