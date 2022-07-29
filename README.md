# AuroraBackend

Internet content aggregator

# Table of content:
<a href="#1">1. Usage pitch</a></br>
<a href="#2">2. How to run</a></br>
<a href="#2">3. Deployment</a></br>

<h2 id="1">1.Usage pitch</h2>
1) Open website</br>
2) Start Searching</br>
3) Receive results without needing to check out every single website on the Internet</br>
<h2 id="2">2. How to run</h2>
<h3>To run complete setup locally: </h3>
<code>
docker-compose -f docker-all-local.yml up
</code>
<h3>To run all of the infrastructure without the application itself (used to debug the app): </h3>
<code>
docker-compose -f docker-local.yml up
</code>
<h3>To run release version using docker hub version of the application (used for deployment): </h3>
<code>
docker-compose -f docker-all.yml up
</code>
<h2 id="3">3. Deployment</h2>
<h3>To deploy the docker container of the application do the following: </h3>
1) Bump version in "version" file (located in the src folder) </br>
2) Navigate to scripts/Docker folder and run: </br>
<code>
release.cmd
</code></br>
The script would create tag for specified version and push updated version of the application to the dockerhub
