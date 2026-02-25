import firebase_admin
from firebase_admin import credentials, firestore
import pandas as pd
import matplotlib.pyplot as plt

# Use a service account
cred = credentials.Certificate('Data/serviceAccountKey.json')
firebase_admin.initialize_app(cred)

# Get a Firestore client
db = firestore.client()

def print_a():
  plt.plot()