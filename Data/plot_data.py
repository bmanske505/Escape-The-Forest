import math
import subprocess

from matplotlib.figure import Figure
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
import os

# This script should be ran from the root "Escape the Forest" folder

CSV_PATH = "Data/tables/"
PNG_PATH = "Data/plots/"

LEVELS = np.arange(0, 7)

############### Utility functions ####################


def make_title(s: str) -> str:
    """
    Converts a string like 'battery_collected' into 'Battery Collected'.
    """
    return " ".join(word.capitalize() for word in s.split("_"))


def csv_to_df(csv_name: str):
    csv_path = os.path.join(CSV_PATH, csv_name)
    return pd.read_csv(csv_path)


def firestore_to_df(
    collection_name: str,
    fields: list[str],
    order_by: str = "level",
    direction: str = "ASC",
    output_filename: str = "output.csv",
):

    cmd = [
        "poetry",
        "run",
        "python",
        "firestore2csv.py",
        "--cred-file",
        "secret.json",
        "--collection-name",
        collection_name,
        "--fields",
        ",".join(fields),
        "--order-by",
        order_by,
        "--direction",
        direction,
    ]

    result = subprocess.run(
        cmd, check=False, capture_output=True, text=True, cwd="Data/firestore2csv"
    )
    print(result.stderr)
    print(result.returncode)
    if result.returncode != 0:
        raise RuntimeError(f"firestore2csv failed: {result.stderr}")

    output_path = os.path.join(CSV_PATH, output_filename)
    with open(output_path, "w") as f:
        f.write(result.stdout)

    return csv_to_df(output_filename)


def fig_to_png(fig: plt.Figure, name: str):
    """
    Saves a matplotlib plot to PNG_PATH with the given name.

    Args:
        plot: The matplotlib.pyplot object (usually plt)
        name (str): File name to save (without path)
    """
    os.makedirs(PNG_PATH, exist_ok=True)  # Ensure directory exists
    path = os.path.join(PNG_PATH, name)
    fig.savefig(path)
    fig.show(fig)
    plt.close(fig)
    print(f"Plot saved to {path}")


############### Defining the actual plots functions ####################


# this is kinda replaced by the violin plot
def stacked_bar():
    df = firestore_to_df("level_complete", ["flashlight_pct_on", "time_spent", "level"])

    # add a new column for overall flashlight info (convert % to seconds)
    df["time_flashlight_on"] = df["flashlight_pct_on"] * df["time_spent"]

    # convert seconds info to minutes
    df["time_spent"] /= 60
    df["time_flashlight_on"] /= 60

    # aggregate data by users
    df = df.groupby("level")[["time_spent", "time_flashlight_on"]].mean()

    fig = plt.figure()
    plt.bar(
        LEVELS + 1,
        df["time_spent"],
        color="navy",
        edgecolor="black",
        label="Exploring Maze",
    )
    plt.bar(
        LEVELS + 1,
        df["time_flashlight_on"],
        color="gold",
        edgecolor="black",
        label="Flashlight On",
    )

    plt.legend()
    plt.title("Flashlight Usage & Time Spent by Level")
    plt.xlabel("Level")
    plt.ylabel("Time (average minutes per user)")
    plt.grid(axis="y", linestyle="--", alpha=0.7)

    fig_to_png(fig, "stacked_bar")


def level_plots():
    df = firestore_to_df(
        "level_complete",
        ["level", "flashlight_pct_on", "time_spent", "userId"],
    )

    # bar plot of user retention
    total_users = df["userId"].nunique()
    print(f"Total users: {total_users}")

    users_per_level = df.groupby("level")["userId"].nunique().sort_index()

    pct_per_level = (users_per_level / total_users) * 100
    fig, ax = plt.subplots()
    ax.plot(pct_per_level.index, pct_per_level.values, marker="o")

    ax.set_title("User Retention across Levels")

    ax.set_xlabel("Level")
    ax.set_ylabel("% of User Base Completed")

    ax.set_xticks(LEVELS)
    ax.set_xticklabels(LEVELS + 1)

    ax.set_ylim(0, 100)
    ax.grid(True, linestyle="--", alpha=0.6)

    fig_to_png(fig, "level_completion_percentage")
    plt.close(fig)

    # --- violin plot: time spent per level ---
    df["time_spent_minutes"] = df["time_spent"] / 60
    time_spent_by_level = []

    # Only include levels that exist in the data
    for lvl in LEVELS:
        data = df[df["level"] == lvl]["time_spent_minutes"].values
        if data.size > 0:
            time_spent_by_level.append(data)
        else:
            time_spent_by_level.append(np.array([0]))  # or skip this level entirely

    # If you skip levels, make a matching positions array
    positions = [lvl for lvl, data in zip(LEVELS, time_spent_by_level) if data.size > 0]

    fig, ax = plt.subplots()
    parts = ax.violinplot(
        time_spent_by_level, positions=positions, showmeans=True, showmedians=False
    )

    for pc in parts["bodies"]:
        pc.set_facecolor("blue")
        pc.set_alpha(0.6)
    ax.set_title("Time Spent Exploring by Level")
    ax.set_xlabel("Level")
    ax.set_ylabel("Time (min. per user)")
    ax.set_xticks(LEVELS)
    ax.set_xticklabels(LEVELS + 1)
    ax.grid(axis="y", linestyle="--", alpha=0.7)
    fig_to_png(fig, "time_violin")
    plt.close(fig)

    # --- pie charts: flashlight on vs off per level ---
    df["time_flashlight_on"] = df["flashlight_pct_on"] * df["time_spent_minutes"]
    df["time_flashlight_off"] = df["time_spent_minutes"] - df["time_flashlight_on"]

    n = len(LEVELS)

    fig, axes = plt.subplots(2, 4)
    axes = axes.flatten()

    if n == 1:
        axes = [axes]

    colors = ["yellow", "darkblue"]

    for ax, lvl in zip(axes, LEVELS):
        level_df = df[df["level"] == lvl]
        avg_on = level_df["time_flashlight_on"].mean()
        avg_off = level_df["time_flashlight_off"].mean()
        ax.pie(
            [avg_on, avg_off],
            colors=colors,
            startangle=90,
            wedgeprops={"edgecolor": "black"},  # <-- black border
        )
        ax.set_title(f"Level {lvl + 1}")

    axes[-1].set_visible(False)

    patches = [
        mpatches.Patch(facecolor=c, edgecolor="black", label=l)
        for c, l in zip(colors, ["On", "Off"])
    ]

    fig.legend(handles=patches, loc="lower right")
    fig.suptitle("Flashlight Use by Level")
    fig_to_png(fig, "flashlight_pie")


def flashlight_plot():
    events = ["flashlight_died", "battery_collected"]
    colors = ["black", "chartreuse"]

    dfs = [
        firestore_to_df(event, ["level", "userId"]).assign(event=event)
        for event in events
    ]
    df: pd.DataFrame = pd.concat(dfs, ignore_index=True)

    # count occurrences per user per level per event, then average across users
    df = df.groupby(["level", "userId", "event"]).size().reset_index(name="count")
    df = df.groupby(["level", "event"])["count"].mean().reset_index(name="mean")

    n_events = len(events)
    width = 0.8 / n_events

    fig, ax = plt.subplots()
    for i, (event, color) in enumerate(zip(events, colors)):
        event_data = (
            df[df["event"] == event].set_index("level").reindex(LEVELS, fill_value=0)
        )
        offsets = LEVELS + (i - n_events / 2 + 0.5) * width
        ax.bar(
            offsets,
            event_data["mean"],
            width=width,
            color=color,
            edgecolor="black",
            label=make_title(event),
        )

    ax.set_title("Average Flashlight Events by Level")
    ax.set_ylabel("# of Occurences (average per user)")
    ax.set_xlabel("Level")
    ax.set_xticks(LEVELS)
    ax.set_xticklabels(LEVELS + 1)
    ax.legend()
    ax.grid(axis="y", linestyle="--", alpha=0.7)

    fig_to_png(fig, "flashlight_bar")


def death_plot():
    events = ["player_died", "stalker_stunned"]
    colors = ["red", "green"]

    dfs = [
        firestore_to_df(event, ["level", "userId"]).assign(event=event)
        for event in events
    ]

    df: pd.DataFrame = pd.concat(dfs, ignore_index=True)
    df = df.groupby(["level", "userId", "event"]).size().reset_index(name="count")
    df = df.groupby(["level", "event"])["count"].mean().reset_index(name="mean")

    n_events = len(events)
    width = 0.8 / n_events

    fig, ax = plt.subplots()
    for i, (event, color) in enumerate(zip(events, colors)):
        event_data = (
            df[df["event"] == event].set_index("level").reindex(LEVELS, fill_value=0)
        )
        offsets = LEVELS + (i - n_events / 2 + 0.5) * width
        ax.bar(
            offsets,
            event_data["mean"],
            width=width,
            color=color,
            edgecolor="black",
            label=make_title(event),
        )

    ax.set_xlabel("Level")
    ax.set_xticks(LEVELS)
    ax.set_xticklabels(LEVELS + 1)
    ax.set_ylabel("# of Occurences (average per user)")
    ax.set_title("Player Death vs. Stuns by Level")
    ax.legend()
    ax.grid(axis="y", linestyle="--", alpha=0.7)

    fig_to_png(fig, "death_bar_plot")


def get_num_users():
    df = firestore_to_df("level_complete", ["userId"])
    count = df["userId"].nunique()
    print(count)
    return count


############### Calling the plots functions ####################

stacked_bar()
level_plots()
flashlight_plot()
death_plot()
