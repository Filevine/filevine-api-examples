# Setup the Virtual Environment and Database

## Install Python Virtual Environment
1. This assumes you already have python installed
2. From a terminal, navigate to this directory `./FV.API.Examples.SamplePartnerApp/FV.API.SamplePartnerAppPy`
3. Execute the python command `python -m venv .venv`
4. Start the virtual environment by running:
   - Mac: `source ./.venv/bin/activate`
   - Windows: `./.venv/Scripts/activate`

## Install Django, Authlib, and other requirements
1. Make sure you are in the same folder as this `README.md` in your terminal.
2. Upgrade pip by executing the python command `python -m pip install --upgrade pip`
3. Execute the python command `python -m pip install -r requirements.txt` to install all required packages
4. Verify by typing `python` to enter the python shell
   1. Enter the following in the shell
      ```
      >>> pip list
      --- A list of installed packages should appear ---
      >>> quit()
      ```
   2. You should see something similar to above for the version of django
4. Django Project
   1. A Django Project already exists called `django_sample`.
   2. You should notice the `manage.py` file. It will be used to start the server as well as other django commands
   
## Setup your dotenv file
_Note: This file is ignored by git as to prevent saving credentials_
1. Create a `.env` file at the root of the `FV.API.SamplePartnerAppPy` folder
2. Add the following lines to the new file:
   ``` dotenv
   DEBUG=True
   SECRET_KEY='{{Your Django Secret Key}}'
   DB_USER='{{ create a user name for the DB }}'
   DB_PASS='{{ create a password for the DB }}'
   CLIENT_ID='{{Client ID}}'
   CLIENT_SECRET='{{Client Secret}}'
   AUTH_CONFIG='https://{{provided-domain}}/.well-known/openid-configuration'
   SCOPES='{{space delimited list of scopes}}'
   GATEWAY_URL='{{URL of the gateway}}'
   ```
3. Change all values necessary

## Setup https for local environment
Setting up https for localhost is fairly easy
1. Install kmcert (Instructions for install can be found here - https://github.com/FiloSottile/mkcert)
2. mkcert -install

## Start the Django Server
1. Ensure your virtual environment is started
   1. Test this by pressing the `Enter` key in the terminal. If it is running you will see `(.venv)` on the line above your prompt
2. Execute the python command `py manage.py runserver_plus --cert-file cert.pem --key-file key.pem localhost:7059`
   1. Note: you can specify the port above or leave it out for a default port of `8000`
   2. You should see something similar to:
      ```
      Django version 5.0.3, using settings 'sample.settings'
      Starting development server at http://127.0.0.1:8000/
      Quit the server with CTRL-BREAK.
      ``` 
3. Verify the server is running
   1. Open a browser and navigate to `https://127.0.0.1:7059/`
   2. You should see `Welcome to the Filevine API Python Sample App`
   3. Similarly, you should be able to navigate to `http://localhost:7059/` and see the same page.
4. Stop the server
5. Migrate the database
   1. Run `python manage.py migrate`. This will create a `db.sqlite3` data file in the `FV.API.SamplePartnerAppPy` directory
   2. Test this by running the server again, `py manage.py runserver_plus --cert-file cert.pem --key-file key.pem localhost:7059`, then navigate to the `admin` path: `https://localhost:7059/admin`
   3. You should see a page asking for a username and password.
   4. To use this administration page, create a super user
      1. Stop the server
      2. Run `python manage.py createsuperuser`
         1. Enter `filevine` as the username
         2. Skip the email address
         3. Enter `filevine` as the password (repeat)
         4. Select `y` to bypass password validation
      5. Run the server again `py manage.py runserver_plus --cert-file cert.pem --key-file key.pem localhost:7059`
      6. Navigate to `https://localhost:7059/admin` and enter `filevine` and log in.
   
# Let's Go!

## Virtual Environment already set up
1. From a terminal, navigate to this directory `./FV.API.Examples.SamplePartnerApp/FV.API.SamplePartnerAppPy`
2. Start the virtual environment by submitting:
   - Mac: `source .venv\bin\activate`
   - Windows:`.venv\Scripts\activate`
3. Start the server by submitting `python manage.py runserver`