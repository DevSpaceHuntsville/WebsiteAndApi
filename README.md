# DevSpace
This is the bestwebsite used for the DevSpace Technical Conference in Huntsville, Alabama.

The project is a .Net WebAPI project.

All pull requests must come to the staging branch. This branch automatically deploys to our staging website, which can be found at https://devspaceconf-staging.azurewebsites.net/  I will merge staging into master after everything can get a smoke test.

The staging website uses a backup of the production database. The production database is copied to the staging database at about 3am every day. If you want to see how that happens, look at the devspace-data project.

The website itself uses SSI for templating. I have the SSI processor running on the standard html pages, so note the special line added to the web.config to add this.

There is a file that is in the .getignore file called analytics.js.  It is the file that contains our Google Analytics code. I'm not worried about people stealing it. You could just grab it off the rendered webpage. However, I don't want people forking the code, forgetting it's there, and acidentially giving us fake analytics.
