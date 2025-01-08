from authlib.integrations.django_client import OAuth
import logging
import sys
import json

from django.http import HttpRequest, JsonResponse, HttpResponse
from django.shortcuts import render, redirect
from django.urls import reverse

from django_sample.settings import CLIENT_ID, CLIENT_SECRET, AUTH_CONFIG, SCOPES, GATEWAY_URL, GET_ME_PATH, \
    GET_USER_ORGS_FOR_TOKEN, API_PATH, DEBUG

log = logging.getLogger('authlib')
log.addHandler(logging.StreamHandler(sys.stdout))
log.setLevel(logging.DEBUG)
logrequests = logging.getLogger('requests')
logrequests.addHandler(logging.StreamHandler(sys.stdout))
logrequests.setLevel(logging.DEBUG)

oauth = OAuth()
oauth.register(
    'filevine',
    client_id=CLIENT_ID,
    client_secret=CLIENT_SECRET,
    server_metadata_url=AUTH_CONFIG,
    client_kwargs={
        'scope': SCOPES,
        'code_challenge_method': 'S256',
    },
)

def home(request):
    user = request.session.get('user')
    if user:
        user = json.dumps(user)
    return render(request, 'home.html', context={'user': user})

def login(request):
    redirect_uri = request.build_absolute_uri(reverse('signin-oidc'))
    return oauth.filevine.authorize_redirect(request, redirect_uri)

def signin_oidc(request):
    token = oauth.filevine.authorize_access_token(request)
    request.session['token'] = token
    return redirect('userinfo/')

def get_userinfo(request):
    token = request.session.get('token')
    if (token is None):
        return redirect('/')

    user_orgs = _get_user_orgs(token)
    native_user_id = user_orgs.get('user').get('userId').get('native')
    org_id = user_orgs.get('orgs')[0].get('orgId')

    org_user = _get_current_user(token, native_user_id, org_id)
    return render(request, 'userinfo.html', context={'orgUser': org_user, 'orgs': user_orgs, 'sessionInfo': sorted(token.get('userinfo').items()), 'tokenItems': sorted(token.items())})


def _get_user_orgs(token):

    url = f'{GATEWAY_URL}/{GET_USER_ORGS_FOR_TOKEN}'

    if DEBUG:
        print("URL", url)
    response = oauth.filevine.post(url, token=token, verify=False)

    if DEBUG:
        print(response)
        print(response.content)

    if response.status_code != 200:
        raise Exception('Error Loading User Orgs')

    return response.json()


def _get_current_user(token, native_user_id, org_id):
    headers = {
        'x-fv-orgid': str(org_id),
        'x-fv-userid': str(native_user_id),
    }
    url = f'{GATEWAY_URL}/{GET_ME_PATH}'

    if DEBUG:
        print("URL", url)
    response = oauth.filevine.get(url, token=token, headers=headers, verify=False)

    if DEBUG:
        print(response)
        print(response.content)

    if response.status_code != 200:
        raise Exception('Error Loading Current User Info')
    return response.json()

def logout(request):
    request.session.clear()
    return redirect('/')
