import pandas as pd
import matplotlib.pyplot as plt
import os

# This script should be ran from the root "Escape the Forest" folder

CSV_PATH = "Data/firestore2csv/local/"
PNG_PATH = "Data/plots/"

############### Utility functions ####################

def get_df_from_csv(csv_name):
    """
    Builds a dataframe from a csv file

    Args:
      csv_name (str): CSV file name inside CSV_PATH
    
    Returns:
      a DataFrame built from the given csv file
    """
    csv_path = os.path.join(CSV_PATH, csv_name)
    return pd.read_csv(csv_path)

def save_plot(plot, name):
    """
    Saves a matplotlib plot to PNG_PATH with the given name.

    Args:
        plot: The matplotlib.pyplot object (usually plt)
        name (str): File name to save (without path)
    """
    os.makedirs(PNG_PATH, exist_ok=True)  # Ensure directory exists
    path = os.path.join(PNG_PATH, name)
    plot.savefig(path)
    #plot.close()  # Close figure to free memory
    print(f"Plot saved to {path}")

############### Defining the actual plots functions ####################

def plot_1():
    df = get_df_from_csv("1.csv")

    # add a new column for overall flashlight info (convert % to seconds)
    df['time_flashlight_on'] = df['flashlight_pct_on'] * df['time_spent']

    # convert seconds info to minutes
    df['time_spent'] /= 60
    df["time_flashlight_on"] /= 60

    # aggregate data by users
    df = df.groupby('level')[['time_spent', 'time_flashlight_on']].mean()
    levels = df.index + 1

    print(df)

    plot = plt.figure()
    plt.bar(levels, df['time_spent'], color='blue', label='Exploring Maze')
    plt.bar(levels, df["time_flashlight_on"], color="orange", label="Flashlight On")

    plt.legend()
    plt.title('Flashlight Usage & Time Spent by Level')
    plt.xlabel('Level')
    plt.ylabel('Time (average minutes per user)')
    plt.grid(axis="y", linestyle="--", alpha=0.7)

    save_plot(plot, "flashlight usage by level")

############### Calling the plots functions ####################

plot_1()
