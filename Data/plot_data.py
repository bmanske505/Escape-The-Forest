import pandas as pd
import matplotlib.pyplot as plt

# --- Step 1: Load CSV ---
csv_path = "firestore2csv/local/1.csv"
df = pd.read_csv(csv_path)

# --- Step 2: Columns & friendly titles ---
x_col = "level"
x_title = "Game Level"

# Numeric y columns
y_cols = ["flashlight_pct_on", "time_spent"]  # you can add timestamp if numeric
y_titles = {
    "flashlight_pct_on": ("Flashlight On", 'red', "% of level"),
    "time_spent": (f"Time Spent", 'blue', "seconds"),
    # "timestamp": "Timestamp (s)"  # optional if numeric
}

# --- Step 3: Compute average across all users per level ---
avg_df = df.groupby(x_col)[y_cols].mean().reset_index()

for y_col in y_cols:
    plt.figure()
    y_title, y_color, y_unit = y_titles[y_col]
    plt.plot(avg_df[x_col] + 1, avg_df[y_col], label=f"Average {y_title}", color=y_color, marker='o')
    plt.xlabel(x_title)
    plt.ylabel(f"{y_title} ({y_unit})")
    plt.title(f"{y_title} over {x_title}")
    plt.legend()
    plt.grid(True)
plt.tight_layout()
plt.show()