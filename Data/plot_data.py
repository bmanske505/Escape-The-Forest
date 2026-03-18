from matplotlib.ticker import PercentFormatter
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
AB_PIPELINES = {0: "Audio", 1: "Visual"}
VERSION_GROUPS = {
    "1.0": "#888888",
    "1.1": "#FFA500",
    "1.2 Audio": "#1f77b4",
    "1.2 Visual": "#2ca02c",
}
THRESHOLD = 10  # number of minutes before an action is considered AFK

############### Utility functions ####################


def get_version_group(row):
    if row["version"] == 1.0:
        return "1.0"
    elif row["version"] == 1.1:
        return "1.1"
    elif row["version"] == 1.2 and row["ab_group"] == 0:
        return "1.2 Audio"
    elif row["version"] == 1.2 and row["ab_group"] == 1:
        return "1.2 Visual"
    return None


def compute_level_times(df):
    """
    Compute time spent per level for each user/playthrough.

    Returns a DataFrame with an added 'level_time' column (minutes).
    Optionally filters out extreme values greater than max_minutes.
    """
    df = df.copy()

    # ensure correct ordering
    df = df.sort_values(["userId", "playthrough", "level"])

    # compute time spent per level (difference between timestamps)
    df["level_time"] = df.groupby(["userId", "playthrough"])["unix_time"].diff()

    # drop level 0 (no previous timestamp)
    df = df[df["level"] > 0]

    # convert to minutes
    df["level_time"] = df["level_time"] / 60

    # optional: remove extreme values
    df = df[df["level_time"] < THRESHOLD]

    return df


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

        # remove ourselves
        df = df[df["userId"] != "0ee2cd15-372c-463d-8986-35ebb91cf660"]

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
    plt.close(fig)
    print(f"Plot saved to {path}")


############### Formatting functions ####################


def make_title(s: str) -> str:
    """
    Converts a string like 'battery_collected' into 'Battery Collected'.
    """
    return " ".join(word.capitalize() for word in s.split("_"))


############### Defining the actual plots functions ####################

def death_plot():
    df = csv_to_df("player_died")

    df["version_group"] = df.apply(get_version_group, axis=1)

    avg_deaths = (
        df.groupby(["level", "version_group", "userId", "playthrough"])
        .size()
        .groupby(["level", "version_group"])
        .mean()
        .reset_index(name="deaths")
    )

    x = np.arange(len(LEVELS)) * 2.5
    width = 0.3

    fig, ax = plt.subplots(figsize=(12, 6))

    ax.grid(axis="y", linestyle="--", alpha=0.6)

    for i, (g, c) in enumerate(VERSION_GROUPS.items()):

        vals = []

        for lvl in LEVELS:
            row = avg_deaths[
                (avg_deaths["level"] == lvl) & (avg_deaths["version_group"] == g)
            ]

            if not row.empty:
                vals.append(row["deaths"].values[0])
            else:
                vals.append(np.nan)  # leaves gap

        ax.bar(x + i * width, vals, width, label=g, color=c)

    # label levels 1–7
    ax.set_xticks(x + 1.5 * width)
    ax.set_xticklabels([lvl + 1 for lvl in LEVELS])

    ax.set_xlabel("Level")
    ax.set_ylabel("Average Deaths per Player")
    ax.set_title("Average Player Deaths by Level and Version")
    ax.legend(title="Version")

    plt.tight_layout()
    fig_to_png(fig, "death_plot")


# TODO: handle issues for this and below functions
# - change level indexing to 1-based instead of 0-based
# - plot ALL levels, not just levels with data
# - make sure to treat same player but different playthrough as unique row when groupby
# - HINT: check the above function for good implementation of these steps


def sibling_hide():
    sibling_hid_df = csv_to_df("sibling_hid")
    sibling_found_df = csv_to_df("sibling_found")

    def compute_differences():
        diffs = []

        # group by user + playthrough
        for (user, playthrough), hide_group in sibling_hid_df.groupby(
            ["userId", "playthrough"]
        ):

            found_group = sibling_found_df[
                (sibling_found_df["userId"] == user)
                & (sibling_found_df["playthrough"] == playthrough)
            ].copy()

            if found_group.empty:
                continue

            found_group = found_group.sort_values("unix_time")
            hide_group = hide_group.sort_values("unix_time")

            used_found = set()

            for _, hide_row in hide_group.iterrows():

                hide_time = hide_row["unix_time"]

                # candidate found events that happen AFTER hide
                candidates = found_group[found_group["unix_time"] > hide_time]

                best_idx = None
                best_time = None

                for idx, found_row in candidates.iterrows():

                    if idx in used_found:
                        continue

                    if best_time is None or found_row["unix_time"] < best_time:
                        best_time = found_row["unix_time"]
                        best_idx = idx

                if best_idx is None:
                    continue

                used_found.add(best_idx)

                diff = (best_time - hide_time) / 60

                if diff <= THRESHOLD:
                    diffs.append(
                        {
                            "level": hide_row["level"],
                            "version_group": get_version_group(hide_row),
                            "minutes": diff,
                        }
                    )

        return pd.DataFrame(diffs)

    diff_df = compute_differences()

    # compute averages
    avg_df = diff_df.groupby(["level", "version_group"])["minutes"].mean().reset_index()

    # --------------------------------------------------
    # plotting
    # --------------------------------------------------

    x = np.arange(len(LEVELS))
    width = 0.2

    fig, ax = plt.subplots()

    for i, (g, c) in enumerate(VERSION_GROUPS.items()):

        vals = []

        for lvl in LEVELS:
            row = avg_df[(avg_df["level"] == lvl) & (avg_df["version_group"] == g)]

            if not row.empty:
                vals.append(row["minutes"].values[0])
            else:
                vals.append(np.nan)

        ax.bar(x + i * width, vals, width, label=g, color=c)

    # label levels 1–7
    ax.set_xticks(x + 1.5 * width)
    ax.set_xticklabels([lvl + 1 for lvl in LEVELS])

    ax.set_xlabel("Level")
    ax.set_ylabel("Average Time (min.)")
    ax.set_title("Average Sibling Search Time by Level and Version")
    ax.legend(title="Version")

    plt.tight_layout()
    fig_to_png(fig, "sibling_hide")


def level_times():
    df = csv_to_df("level_complete")
    df = compute_level_times(df)

    # add version_group column
    df["version_group"] = df.apply(get_version_group, axis=1)

    fig, ax = plt.subplots(figsize=(10, 6))

    # loop through all version groups defined in VERSION_GROUPS
    for version_group, color in VERSION_GROUPS.items():
        df_vg = df[df["version_group"] == version_group]

        if df_vg.empty:
            continue

        # average level_time per level
        avg_times = df_vg.groupby("level")["level_time"].mean().sort_index()

        ax.plot(
            avg_times.index,
            avg_times.values,
            marker="o",
            linestyle="-",  # can change to "--" if you want dashes
            color=color,
            label=version_group,
        )

    ax.set_xlabel("Level")
    ax.set_ylabel("Average Time (minutes)")

    # label levels 1–7
    ax.set_xticklabels([lvl + 1 for lvl in LEVELS])

    ax.legend(title="Version")
    ax.grid(True, linestyle="--", alpha=0.5)

    ax.set_title("Average Maze Solve Time by Level and Version")

    plt.tight_layout()
    fig_to_png(fig, "level_times")


def user_retention():
    df = csv_to_df("level_complete")

    # create a version_group column using your function
    df["version_group"] = df.apply(get_version_group, axis=1)

    fig, ax = plt.subplots(figsize=(12, 6))

    for version_group, color in VERSION_GROUPS.items():
        df_vg = df[df["version_group"] == version_group]
        total_users = df_vg["userId"].nunique()
        if total_users == 0:
            continue

        users_per_level = df_vg.groupby("level")["userId"].nunique().sort_index()
        # restrict to levels 0–6
        users_per_level = users_per_level.loc[users_per_level.index.isin(LEVELS)]

        pct_per_level = (users_per_level / total_users) * 100

        ax.plot(
            pct_per_level.index,
            pct_per_level.values,
            marker="o",
            color=color,
            label=version_group,
        )

    ax.set_title("User Retention by Level and Version")
    ax.set_xlabel("Level")
    ax.set_ylabel("% of User Base Completed")
    ax.yaxis.set_major_formatter(PercentFormatter())

    # label levels 1–7
    ax.set_xticks(LEVELS)  # positions
    ax.set_xticklabels([lvl + 1 for lvl in LEVELS])

    ax.set_ylim(0, 100)
    ax.grid(True, linestyle="--", alpha=0.6)

    ax.legend(title="Version")

    plt.tight_layout()
    fig_to_png(fig, "user_retention")

def print_num_users():
    df = csv_to_df("level_complete")
    df["version_group"] = df.apply(get_version_group, axis=1)
    # nunique per version_group
    df = df.groupby("version_group")["userId"].nunique()

    print("\nUSER DEMOGRAPHIC:")
    for group, count in df.items():
        print(f"Version {group}: {count} users")
    print()


############### Calling the plots functions ####################

def main():
  pull_data()  # this refreshes the csv files from firestore
  print_num_users()

  user_retention()
  level_times()
  sibling_hide()
  death_plot()

# main()

df = csv_to_df("level_complete")

# Calculate unique users in version 1.0 and 1.2
users_v1_0 = set(df[df["version"] == 1.0]["userId"])
users_v1_2 = set(df[df["version"] == 1.2]["userId"])

print(len(users_v1_0))
print(len(users_v1_2))
# Find users in both versions and only in version 1.0
common_users = len(users_v1_0 & users_v1_2)
print(common_users)
unique_to_v1_0 = len(users_v1_0 - users_v1_2)
print(unique_to_v1_0)
# Create a DataFrame with the results
df = pd.DataFrame(
    {"Answer": [common_users, unique_to_v1_0]},
    index=["In both versions", "Only in version 1.0"],
)
