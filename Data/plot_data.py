import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
import os
from matplotlib import pyplot as plt
import pandas as pd
from google.cloud import firestore

# This script should be ran from the root "Escape the Forest" folder

LEVELS = np.arange(0, 7)
CSV_PATH = "Data/tables/"
PNG_PATH = "Data/plots/"
COLLECTIONS = [
    "battery_collected",
    "echo_used",
    "flashlight_died",
    "level_complete",
    "player_died",
    "sibling_found",
    "sibling_hid",
    "stalker_stunned",
]
VERSIONS = [1.0, 1.1, 1.2]

############### Utility functions ####################


def flatten_dict(d, parent_key="", sep="."):
    items = []
    for k, v in d.items():
        new_key = f"{parent_key}{sep}{k}" if parent_key else k
        if isinstance(v, dict):
            items.extend(flatten_dict(v, new_key, sep=sep).items())
        else:
            items.append((new_key, v))
    return dict(items)


def pull_data(
    collection_names: list[str] = COLLECTIONS,
    cred_path: str = "Data/secret.json",
):
    os.environ["GOOGLE_APPLICATION_CREDENTIALS"] = cred_path
    os.makedirs(CSV_PATH, exist_ok=True)

    db = firestore.Client()

    for collection_name in collection_names:
        rows = []

        for doc in db.collection(collection_name).stream():
            data = doc.to_dict()
            if not data:
                continue

            flat = flatten_dict(data)
            flat["_doc_id"] = doc.id  # optional but VERY useful
            rows.append(flat)

        if not rows:
            print(f"Skipping empty collection: {collection_name}")
            continue

        df = pd.DataFrame(rows)

        output_path = os.path.join(CSV_PATH, f"{collection_name}.csv")
        df.to_csv(output_path, index=False)

        print(f"Saved {output_path} ({len(df)} rows)")


def csv_to_df(collection_name: str):
    csv_path = os.path.join(CSV_PATH, f"{collection_name}.csv")
    return pd.read_csv(csv_path)


def fig_to_png(fig: plt.Figure, name: str):
    """
    Saves a matplotlib plot to PNG_PATH with the given name.

    Args:
        plot: The matplotlib.pyplot object (usually plt)
        name (str): File name to save (without path)
    """
    os.makedirs(PNG_PATH, exist_ok=True)  # Ensure directory exists
    path = os.path.join(PNG_PATH, f"{name}.png")
    fig.savefig(path)
    fig.show(fig)
    plt.close(fig)
    print(f"Plot saved to {path}")


############### Formatting functions ####################


def make_title(s: str) -> str:
    """
    Converts a string like 'battery_collected' into 'Battery Collected'.
    """
    return " ".join(word.capitalize() for word in s.split("_"))


############### Defining the actual plots functions ####################


# this is kinda replaced by the violin plot
def stacked_bar(version: float):
    df = csv_to_df("level_complete")
    df = df[df["version"] == version]  # filter for the version

    # add a new column for overall flashlight info (convert % to seconds)
    df["time_flashlight_on"] = df["flashlight_pct_on"] * df["time_spent"]

    # convert seconds info to minutes
    df["time_spent"] /= 60
    df["time_flashlight_on"] /= 60

    # aggregate data by users
    df = (
        df.groupby("level")[["time_spent", "time_flashlight_on"]]
        .mean()
        .reindex(LEVELS, fill_value=0)
    )

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
    plt.title(f"Flashlight Usage & Time Spent by Level - Version {str(version)}")
    plt.xlabel("Level")
    plt.ylabel("Time (average minutes per user)")
    plt.grid(axis="y", linestyle="--", alpha=0.7)

    fig_to_png(fig, f"stacked_bar_v{str(version)}")


def new_plot():
    df = csv_to_df("player_died")
    df["type"] = df["type"].fillna("Stalker")

    # --- deaths per player ---
    deaths_per_player = (
        df.groupby(["level", "version", "userId", "type"])
        .size()
        .reset_index(name="deaths")
    )

    # --- average deaths across players ---
    avg_deaths = (
        deaths_per_player.groupby(["level", "version", "type"])["deaths"]
        .mean()
        .unstack(fill_value=0)
    )

    levels = sorted(df["level"].unique())

    # rows = versions, cols = levels
    fig, axes = plt.subplots(
        len(VERSIONS),
        len(levels),
        figsize=(3 * len(levels), 4 * len(VERSIONS)),
        sharey=True,
    )

    if len(VERSIONS) == 1:
        axes = [axes]

    for i, version in enumerate(VERSIONS):
        for j, level in enumerate(levels):
            ax = axes[i][j]

            if (level, version) in avg_deaths.index:
                row = avg_deaths.loc[(level, version)]
                dog = row.get("Dog", 0)
                stalker = row.get("Stalker", 0)
            else:
                dog = 0
                stalker = 0

            ax.bar([0], stalker, color="red", label="Stalker")
            ax.bar([0], dog, bottom=[stalker], color="orange", label="Dog")

            ax.set_xticks([])
            ax.set_title(f"Level {level} | v{version}")

            if j == 0:
                ax.set_ylabel("Avg Deaths per Player")

    handles, labels = axes[0][0].get_legend_handles_labels()
    fig.legend(handles, labels, loc="upper right")

    plt.tight_layout()

    fig_to_png(fig, "new_plot_both_versions")
    plt.close(fig)


def level_plots(version: float):
    df = csv_to_df("level_complete")

    df = df[df["version"] == version]  # filter for the version

    # bar plot of user retention
    total_users = df["userId"].nunique()
    print(f"Total users: {total_users}")

    users_per_level = df.groupby("level")["userId"].nunique().sort_index()

    pct_per_level = (users_per_level / total_users) * 100
    fig, ax = plt.subplots()
    ax.plot(pct_per_level.index, pct_per_level.values, marker="o")

    ax.set_title(f"User Retention across Levels - Version {str(version)}")

    ax.set_xlabel("Level")
    ax.set_ylabel("% of User Base Completed")

    ax.set_xticks(LEVELS)
    ax.set_xticklabels(LEVELS + 1)

    ax.set_ylim(0, 100)
    ax.grid(True, linestyle="--", alpha=0.6)

    fig_to_png(fig, f"level_completion_percentage_v{str(version)}")
    plt.close(fig)

    # --- violin plot: time spent per level ---
    df["time_spent_minutes"] = df["time_spent"] / 60
    time_spent_by_level = []

    # Only include levels that exist in the data
    for lvl in LEVELS:
        data = df["time_spent_minutes"].values
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
    ax.set_title(f"Time Spent Exploring by Level - Version {str(version)}")
    ax.set_xlabel("Level")
    ax.set_ylabel("Time (min. per user)")
    ax.set_xticks(LEVELS)
    ax.set_xticklabels(LEVELS + 1)
    ax.grid(axis="y", linestyle="--", alpha=0.7)
    fig_to_png(fig, f"time_violin_v{str(version)}")
    plt.close(fig)

    # --- pie charts: flashlight on vs off per level ---
    """
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
    fig.suptitle(f"Flashlight Use by Level - Version {str(version)}")
    fig_to_png(fig, f"flashlight_pie_v{str(version)}")
    """


def flashlight_plot(version: float):
    events = ["flashlight_died"]
    colors = ["black"]

    dfs = [csv_to_df(event).assign(event=event) for event in events]
    df: pd.DataFrame = pd.concat(dfs, ignore_index=True)
    df = df[df["version"] == version]  # filter for the version

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

    ax.set_title(f"Average Flashlight Events by Level - Version {str(version)}")
    ax.set_ylabel("# of Occurences (average per user)")
    ax.set_xlabel("Level")
    ax.set_xticks(LEVELS)
    ax.set_xticklabels(LEVELS + 1)
    ax.legend()
    ax.grid(axis="y", linestyle="--", alpha=0.7)

    fig_to_png(fig, f"flashlight_bar_v{str(version)}")


def death_plot(version: float):
    events = ["player_died", "stalker_stunned"]
    colors = ["red", "green"]

    dfs = [csv_to_df(event).assign(event=event) for event in events]

    df: pd.DataFrame = pd.concat(dfs, ignore_index=True)
    df = df[df["version"] == version]  # filter for the version

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
    ax.set_title(f"Player Death vs. Stuns by Level - Version {str(version)}")
    ax.legend()
    ax.grid(axis="y", linestyle="--", alpha=0.7)

    fig_to_png(fig, f"death_bar_plot_v{str(version)}")

def sibling_found_plot(version: float):
    df_hid = csv_to_df("sibling_hid")
    df_found = csv_to_df("sibling_found")
    # Filter for version 1.1 and drop rows with missing unix_time or level
    df_hid = df_hid[df_hid["version"] == version]
    df_found = df_found[df_found["version"] == version]

    # Merge on level, userId, and playthrough if possible
    merge_cols = ["level", "userId"]
    if "playthrough" in df_hid.columns and "playthrough" in df_found.columns:
        merge_cols.append("playthrough")
    merged = pd.merge(
        df_hid,
        df_found,
        on=merge_cols,
        suffixes=("_hid", "_found"),
        how="inner",
        validate="many_to_many",
    )
    print(f"Merged rows: {len(merged)}")
    # Compute time to find
    merged["time_to_find"] = merged["unix_time_found"] - merged["unix_time_hid"]

    # Group by level and average
    avg_time = merged.groupby("level")["time_to_find"].mean()

    fig, ax = plt.subplots()
    ax.bar(avg_time.index, avg_time.values)
    ax.set_xlabel("Level")
    ax.set_ylabel("Time Spend to Find Sibling (average minutes per user)")
    ax.set_title(f"Average Time to Find Sibling by Level - Version {str(version)}")
    ax.set_xticks(avg_time.index)
    ax.grid(axis="y", linestyle="--", alpha=0.5)
    fig_to_png(fig, f"sibling_found_plot_v{str(version)}")


def print_num_users():
    df = csv_to_df("level_complete")
    for version in VERSIONS:
        print(f"VERSION {str(version)}")
        filtered = df[df["version"] == version]
        unique_users = filtered["userId"].nunique()

        print(unique_users)


############### Calling the plots functions ####################

pull_data() # this refreshes the csv files from firestore
print_num_users()
# new_plot()
# sibling_found_plot(1.1)
# sibling_found_plot(1.0)

"""
for version in VERSIONS:
    stacked_bar(version)
    level_plots(version)
    flashlight_plot(version)
    death_plot(version)
    sibling_found_plot(version)
"""
