# XLoad

## Overview

This code will run a specified code X amount of times using simplex noise generation to somewhat generate a natural load on the code. 
At this point in time, for this to work, insert your code into << INSERT CODE HERE >>
     
## Future work
- Run .dll code instead of replacing code
- Log to file instead of console
- Better sample image generation
  
## Arguments
### General
   - -dryrun
     - Do not automatically start the load
### Noise Generation
   - -scale <float>
     - Scale for the Simplex Noise generation
     - Default : 0.001
   
   - -seed <int> 
     - Seed for the Simplex Noise generation
     - Default : 1337
   
   - -resolution <int>
     - Frequency (in seconds) for which a new point is generated 
     - Default: 60 (one minute)       
	
### Load Generation
   - -time <int>
     - Amount of seconds for the system to run
     - Default : 86400 (one day)
   - -infite
     - If this argument is used, the system will continue to operate after the -time
   - -requests <int>
     - Number of requests to be performed in -time
     - Default : 1000000
   - -maxtasks <int>
     - Maximum amount of TPL tasks the system will generate
   - -mintasks <int>
     - Minimum amount of TPL tasks the system will generate
### Image Generation
   - -image <file path>
     - Path for the creation of a bmp file with the generated graph
